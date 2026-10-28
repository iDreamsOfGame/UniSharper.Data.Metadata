// A cross-platform console program that tests the MetadataDatabaseFileGenerator and the metadata
// database files outside the Unity editor. The Unity engine APIs used by the editor assemblies are
// substituted by stub assemblies (see the UnityStubs folder).
//
// The test flow:
//   1. Reads all Excel workbook files (*.xls, *.xlsx) from the 'SampleExcelFiles' folder and parses
//      them to build the expected data (the test oracle), replicating the conversion rules of the
//      metadata importing pipeline.
//   2. Generates a single database file by MetadataDatabaseFileGenerator.GenerateDatabaseFile
//      (encrypted + compressed) for the first excel workbook file.
//   3. Generates database files in batch by MetadataDatabaseFileGenerator.GenerateDatabaseFiles (plain).
//   4. Reads the generated database files back by ReSharp.Data.IBoxDB.IBoxDBAdapter and verifies all
//      data. The entity types for reading are created dynamically by Reflection.Emit, just like the
//      generator does. The wrapped database files are unwrapped (decrypted + decompressed) with the
//      default database providers before reading.
//   5. Emphasizes the verification of the Unity type data (Vector2/Vector3/etc. and their arrays) and
//      the custom enum type data: the Unity type cells are parsed by the real UniSharper parse
//      utilities into Unity structs, so both the component values and their order are verified; the
//      enum values are verified with the index and name mapping of the enum value lists.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using ExcelDataReader;
using ReSharp.Data.IBoxDB;
using ReSharp.Security.Cryptography;
using UniSharper;
using UniSharper.Data.Metadata;
using UniSharper.Data.Metadata.Providers;
using UniSharper.Extensions;
using UniSharperEditor.Data.Metadata;
using ColorUtility = UniSharper.Extensions.ColorUtility;

namespace MetadataDatabaseFileGeneratorTest
{
    internal static class Program
    {
        private const string SampleExcelFilesFolderName = "SampleExcelFiles";

        private const string DatabaseFileExtension = FileExtensions.DatabaseFile + FileExtensions.UnityBinaryAssetFile;

        private const int CommentRowIndex = 0;

        private const int TypeRowIndex = 1;

        private const int NameRowIndex = 2;

        private const int DataStartingRowIndex = 3;

        private const char ArrayElementSeparator = '|';

        private static readonly ExcelDataParseOptions Options = new(CommentRowIndex,
            TypeRowIndex,
            NameRowIndex,
            DataStartingRowIndex,
            ArrayElementSeparator);

        // The default database providers, they must be the ones the generator used to wrap the database files.
        private static readonly IDatabaseCryptoProvider CryptoProvider = new AesCbcCryptoProvider();

        private static readonly IDatabaseCompressionProvider CompressionProvider = new LZ4CompressionProvider();

        private static readonly MethodInfo AdapterEnsureTableMethod = Array.Find(typeof(IBoxDBAdapter).GetMethods(BindingFlags.Public | BindingFlags.Instance),
            method => method.Name == nameof(IBoxDBAdapter.EnsureTable)
                      && method.IsGenericMethodDefinition
                      && method.GetParameters().Length == 2
                      && method.GetParameters()[0].ParameterType == typeof(string)
                      && method.GetParameters()[1].ParameterType == typeof(string));

        private static readonly MethodInfo AdapterGetAllMethod = Array.Find(typeof(IBoxDBAdapter).GetMethods(BindingFlags.Public | BindingFlags.Instance),
            method => method.Name == nameof(IBoxDBAdapter.GetAll)
                      && method.IsGenericMethodDefinition
                      && method.GetParameters().Length == 1
                      && method.GetParameters()[0].ParameterType == typeof(string));

        private static readonly MethodInfo AdapterGetMethod = Array.Find(typeof(IBoxDBAdapter).GetMethods(BindingFlags.Public | BindingFlags.Instance),
            method => method.Name == nameof(IBoxDBAdapter.Get)
                      && method.IsGenericMethodDefinition
                      && method.GetParameters().Length == 2
                      && method.GetParameters()[0].ParameterType == typeof(string)
                      && method.GetParameters()[1].ParameterType == typeof(object[]));

        private static readonly Dictionary<string, Type> PlainTypeMap = new()
        {
            { "string", typeof(string) },
            { "bool", typeof(bool) },
            { "byte", typeof(byte) },
            { "sbyte", typeof(sbyte) },
            { "decimal", typeof(decimal) },
            { "double", typeof(double) },
            { "float", typeof(float) },
            { "int", typeof(int) },
            { "uint", typeof(uint) },
            { "long", typeof(long) },
            { "ulong", typeof(ulong) },
            { "short", typeof(short) },
            { "ushort", typeof(ushort) }
        };

        // Unity type name -> (value array storage type, component count).
        private static readonly Dictionary<string, Tuple<Type, int>> UnityTypeMap = new()
        {
            { "Vector2", Tuple.Create(typeof(float[]), 2) },
            { "Vector2Int", Tuple.Create(typeof(int[]), 2) },
            { "Vector3", Tuple.Create(typeof(float[]), 3) },
            { "Vector3Int", Tuple.Create(typeof(int[]), 3) },
            { "Vector4", Tuple.Create(typeof(float[]), 4) },
            { "RangeInt", Tuple.Create(typeof(int[]), 2) },
            { "Quaternion", Tuple.Create(typeof(float[]), 4) },
            { "Rect", Tuple.Create(typeof(float[]), 4) },
            { "RectInt", Tuple.Create(typeof(int[]), 4) },
            { "Color", Tuple.Create(typeof(float[]), 4) },
            { "Color32", Tuple.Create(typeof(byte[]), 4) }
        };

        // Unity type name -> the semantic names of its components, used to verify the component order.
        private static readonly Dictionary<string, string[]> UnityComponentNameMap = new()
        {
            { "Vector2", ["x", "y"] },
            { "Vector2Int", ["x", "y"] },
            { "Vector3", ["x", "y", "z"] },
            { "Vector3Int", ["x", "y", "z"] },
            { "Vector4", ["x", "y", "z", "w"] },
            { "RangeInt", ["start", "length"] },
            { "Quaternion", ["x", "y", "z", "w"] },
            { "Rect", ["x", "y", "width", "height"] },
            { "RectInt", ["x", "y", "width", "height"] },
            { "Color", ["r", "g", "b", "a"] },
            { "Color32", ["r", "g", "b", "a"] }
        };

        private static int failureCount;

        private static int mismatchPrinted;
        
        private static int Main()
        {
            try
            {
                Debugger.Logger = new ConsoleLogger();

                CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
                CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

                // ExcelDataReader requires the code pages encoding provider on .NET Core to read Excel files.
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                var baseDirectory = AppContext.BaseDirectory;

                // The generator writes temporary database files under the current directory.
                Directory.SetCurrentDirectory(baseDirectory);

                // Step 1: locate the 'SampleExcelFiles' folder and collect the Excel workbook files.
                var sampleFilesFolderPath = FindSampleExcelFilesFolder(baseDirectory);
                Check(sampleFilesFolderPath != null, $"Found '{SampleExcelFilesFolderName}' folder");
                if (sampleFilesFolderPath == null)
                    return PrintSummary();

                var excelFilePaths = Directory.GetFiles(sampleFilesFolderPath)
                    .Where(IsExcelWorkbookFile)
                    .OrderBy(Path.GetFileName)
                    .ToArray();
                Check(excelFilePaths.Length > 0, $"Found {excelFilePaths.Length} excel workbook file(s) in '{sampleFilesFolderPath}'");
                if (excelFilePaths.Length == 0)
                    return PrintSummary();

                // Step 2: parse the Excel workbook files to build the expected data (the test oracle).
                PrintStep("Parsing excel workbook files to build expected data...");
                var expectedEntities = new List<ExpectedEntity>();
                foreach (var excelFilePath in excelFilePaths)
                {
                    var model = ParseExpectedEntity(excelFilePath);
                    if (model != null)
                        expectedEntities.Add(model);
                }

                Check(expectedEntities.Count == excelFilePaths.Length, "All excel workbook files parsed to expected data");
                if (expectedEntities.Count != excelFilePaths.Length)
                    return PrintSummary();

                var singleOutputFolderPath = Path.Combine(baseDirectory, "Output", "Single");
                var batchOutputFolderPath = Path.Combine(baseDirectory, "Output", "Batch");
                ResetDirectory(singleOutputFolderPath);
                ResetDirectory(batchOutputFolderPath);

                // Step 3: generate a single database file (encrypted + compressed) for the first excel workbook file.
                PrintStep($"Generating single database file for '{Path.GetFileName(excelFilePaths[0])}' (encrypted + compressed)...");
                var singleResult = MetadataDatabaseFileGenerator.GenerateDatabaseFile(excelFilePaths[0],
                    singleOutputFolderPath,
                    Options,
                    false,
                    false);
                Check(singleResult, "GenerateDatabaseFile returns true");

                // Step 4: generate database files in batch (plain).
                PrintStep("Generating database files in batch (plain)...");
                var batchResult = MetadataDatabaseFileGenerator.GenerateDatabaseFiles(sampleFilesFolderPath,
                    batchOutputFolderPath,
                    Options,
                    true,
                    true,
                    null,
                    CompressionProvider);
                Check(batchResult, "GenerateDatabaseFiles returns true");
                if (!singleResult || !batchResult)
                    return PrintSummary();

                foreach (var model in expectedEntities)
                {
                    Check(File.Exists(Path.Combine(batchOutputFolderPath, model.EntityClassName + DatabaseFileExtension)),
                        $"'{model.EntityClassName}{DatabaseFileExtension}' exists in batch output folder");
                }

                Check(File.Exists(Path.Combine(batchOutputFolderPath, "MetadataEntityDBConfig" + DatabaseFileExtension)),
                    $"'MetadataEntityDBConfig{DatabaseFileExtension}' exists in batch output folder");

                // Step 5: read the generated database files back by IBoxDBAdapter. The config database of
                // the batch output contains the config records of all entities, and the database file of
                // the first entity comes from the single output (encrypted + compressed), so both provider
                // paths of the runtime get verified.
                PrintStep("Reading database files back by IBoxDBAdapter...");

                var configFilePath = Path.Combine(batchOutputFolderPath, "MetadataEntityDBConfig" + DatabaseFileExtension);
                var configAdapterRawData = UnwrapDatabaseFile(File.ReadAllBytes(configFilePath));
                using (var configAdapter = new IBoxDBAdapter(configAdapterRawData))
                {
                    configAdapter.EnsureTable<MetadataEntityDBConfig>(MetadataEntityDBConfig.TableName, MetadataEntityDBConfig.TablePrimaryKey);
                    configAdapter.Open();

                    foreach (var model in expectedEntities)
                    {
                        var config = configAdapter.Get<MetadataEntityDBConfig>(MetadataEntityDBConfig.TableName, model.TableName);
                        Check(config != null, $"{model.EntityClassName}: config record exists for table '{model.TableName}'");
                        if (config != null)
                        {
                            Check(config.PrimaryKey == model.PrimaryKeyPropertyName,
                                $"{model.EntityClassName}: config primary key is '{model.PrimaryKeyPropertyName}' (actual: '{config.PrimaryKey}')");
                        }
                    }
                }

                for (var i = 0; i < expectedEntities.Count; i++)
                {
                    var model = expectedEntities[i];
                    var outputFolderPath = i == 0 ? singleOutputFolderPath : batchOutputFolderPath;
                    var adapterRawData = UnwrapDatabaseFile(File.ReadAllBytes(Path.Combine(outputFolderPath, model.EntityClassName + DatabaseFileExtension)));
                    using var adapter = new IBoxDBAdapter(adapterRawData);
                    AdapterEnsureTableMethod.MakeGenericMethod(model.EntityType).Invoke(adapter, [model.TableName, model.PrimaryKeyPropertyName]);
                    adapter.Open();
                    VerifyEntity(model, adapter);
                }

                return PrintSummary();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private static string FindSampleExcelFilesFolder(string startDirectory)
        {
            var directory = new DirectoryInfo(startDirectory);
            for (var depth = 0; depth < 8 && directory != null; depth++, directory = directory.Parent)
            {
                var candidate = Path.Combine(directory.FullName, SampleExcelFilesFolderName);
                if (Directory.Exists(candidate))
                    return candidate;
            }

            return null;
        }

        private static bool IsExcelWorkbookFile(string filePath)
        {
            var extension = Path.GetExtension(filePath);
            return extension.Equals(FileExtensions.ExcelSpreadsheetFile, StringComparison.OrdinalIgnoreCase)
                   || extension.Equals(FileExtensions.ExcelWorkbookFile, StringComparison.OrdinalIgnoreCase);
        }

        private static DataTable ReadExcelFirstTable(string filePath)
        {
            using var stream = File.OpenRead(filePath);
            using var reader = ExcelReaderFactory.CreateReader(stream);
            var dataSet = reader.AsDataSet();
            return dataSet is { Tables: { Count: > 0 } } ? dataSet.Tables[0] : null;
        }

        private static ExpectedEntity ParseExpectedEntity(string excelFilePath)
        {
            var fileName = Path.GetFileNameWithoutExtension(excelFilePath);
            var table = ReadExcelFirstTable(excelFilePath);
            Check(table != null, $"Excel file '{fileName}' has a worksheet");
            if (table == null)
                return null;

            var entityClassName = ToTitleCase(fileName);
            var properties = new List<ExpectedProperty>();

            for (var column = 0; column < table.Columns.Count; column++)
            {
                var propertyName = table.Rows[NameRowIndex][column].ToString();
                var propertyType = table.Rows[TypeRowIndex][column].ToString();
                if (string.IsNullOrEmpty(propertyName) || string.IsNullOrEmpty(propertyType))
                    continue;

                propertyName = ToTitleCase(propertyName.Trim());
                propertyType = propertyType.Trim();
                if (!TryResolvePropertyType(propertyType, out var storageType, out var valueKind, out var unityComponentCount, out var unityIsArray))
                {
                    Check(false, $"Property type '{propertyType}' of '{propertyName}' in '{fileName}' is not supported");
                    continue;
                }

                properties.Add(new ExpectedProperty
                {
                    PropertyName = valueKind == "Enum" || valueKind == "Unity" ? propertyName + "Value" : propertyName,
                    OriginalPropertyName = propertyName,
                    SourceColumn = column,
                    SourceTypeString = propertyType,
                    StorageType = storageType,
                    UnityComponentCount = unityComponentCount,
                    UnityIsArray = unityIsArray,
                    ValueKind = valueKind
                });
            }

            Check(properties.Count > 0, $"'{fileName}' has valid property definitions");
            if (properties.Count == 0)
                return null;

            // Build the enum value lists, index 0 is 'None', then the cell values by order of appearance.
            foreach (var property in properties.Where(property => property.ValueKind == "Enum"))
            {
                property.EnumValues = ["None"];
                for (var row = DataStartingRowIndex; row < table.Rows.Count; row++)
                {
                    var cell = table.Rows[row][property.SourceColumn].ToString()?.Trim();
                    if (!string.IsNullOrEmpty(cell) && !property.EnumValues.Contains(cell))
                        property.EnumValues.Add(cell);
                }
            }

            // Build the expected rows.
            var rows = new List<Dictionary<string, object>>();
            var rawRows = new List<Dictionary<string, string>>();
            for (var row = DataStartingRowIndex; row < table.Rows.Count; row++)
            {
                var expectedValues = new Dictionary<string, object>();
                var rawValues = new Dictionary<string, string>();
                foreach (var property in properties)
                {
                    var cell = table.Rows[row][property.SourceColumn].ToString();
                    rawValues[property.PropertyName] = cell;
                    expectedValues[property.PropertyName] = ConvertCell(property, cell);
                }

                rows.Add(expectedValues);
                rawRows.Add(rawValues);
            }

            // Emit the entity type for IBoxDBAdapter reading, just like the generator does for writing.
            var entityType = CreateEntityType(entityClassName, properties);
            Console.WriteLine($"  {entityClassName}: {properties.Count} properties, {rows.Count} data rows");

            return new ExpectedEntity
            {
                EntityClassName = entityClassName,
                EntityType = entityType,
                TableName = GetRealTableName(entityClassName),
                PrimaryKeyPropertyName = properties[0].PropertyName,
                Properties = properties,
                Rows = rows,
                RawRows = rawRows
            };
        }

        private static bool TryResolvePropertyType(string typeString,
            out Type storageType,
            out string valueKind,
            out int unityComponentCount,
            out bool unityIsArray)
        {
            storageType = null;
            valueKind = "Plain";
            unityComponentCount = 0;
            unityIsArray = false;

            if (typeString == "enum")
            {
                valueKind = "Enum";
                storageType = typeof(int);
                return true;
            }

            if (UnityTypeMap.TryGetValue(typeString, out var unity))
            {
                valueKind = "Unity";
                unityComponentCount = unity.Item2;
                storageType = unity.Item1;
                return true;
            }

            if (typeString.EndsWith("[]", StringComparison.Ordinal))
            {
                var elementString = typeString.Substring(0, typeString.Length - 2);
                if (UnityTypeMap.TryGetValue(elementString, out var unityElement))
                {
                    valueKind = "Unity";
                    unityComponentCount = unityElement.Item2;
                    unityIsArray = true;
                    storageType = unityElement.Item1;
                    return true;
                }

                if (PlainTypeMap.TryGetValue(elementString, out var elementType))
                {
                    valueKind = "Array";
                    storageType = elementType.MakeArrayType();
                    return true;
                }

                return false;
            }

            if (PlainTypeMap.TryGetValue(typeString, out var plain))
            {
                valueKind = "Plain";
                storageType = plain;
                return true;
            }

            return false;
        }

        private static object ConvertCell(ExpectedProperty property, string cell)
        {
            var value = cell.Trim();
            switch (property.ValueKind)
            {
                case "Enum":
                {
                    // An empty enum cell maps to the index 0 ('None'), otherwise to the index in the value list.
                    return string.IsNullOrEmpty(value) ? 0 : property.EnumValues.IndexOf(value);
                }
                case "Unity":
                    return ConvertUnityValue(property, value);
                case "Array":
                {
                    var elementType = property.StorageType.GetElementType();
                    var elementStrings = string.IsNullOrEmpty(value)
                        ? []
                        : value.Split([ArrayElementSeparator], StringSplitOptions.RemoveEmptyEntries);
                    var array = Array.CreateInstance(elementType, elementStrings.Length);
                    for (var i = 0; i < elementStrings.Length; i++)
                        array.SetValue(ConvertPlainValue(elementType, elementStrings[i]), i);

                    return array;
                }
                default:
                    return ConvertPlainValue(property.StorageType, value);
            }
        }

        private static object ConvertUnityValue(ExpectedProperty property, string value)
        {
            var elementType = property.StorageType.GetElementType();
            var baseTypeName = property.UnityIsArray
                ? property.SourceTypeString[..^2]
                : property.SourceTypeString;
            var components = new List<object>();

            if (property.UnityIsArray)
            {
                // An empty array cell maps to an empty array, otherwise the elements are parsed by the
                // real Unity type parse utilities, just like the converters of the importing pipeline.
                if (!string.IsNullOrEmpty(value))
                    AppendUnityArrayComponents(components, baseTypeName, value);
            }
            else
            {
                // An empty single Unity type cell maps to the default components (all zeros).
                AppendUnityComponents(components, baseTypeName, value);
            }

            var array = Array.CreateInstance(elementType, components.Count);
            for (var i = 0; i < components.Count; i++)
                array.SetValue(components[i], i);

            return array;
        }

        private static void AppendUnityComponents(List<object> components, string baseTypeName, string elementString)
        {
            // The cell values are parsed by the real UniSharper parse utilities, so the expected
            // components are exactly what the Unity structs contain after parsing.
            switch (baseTypeName)
            {
                case "Vector2":
                    Vector2Utility.TryParse(elementString, out var vector2);
                    components.Add(vector2.x);
                    components.Add(vector2.y);
                    break;
                case "Vector2Int":
                    Vector2IntUtility.TryParse(elementString, out var vector2Int);
                    components.Add(vector2Int.x);
                    components.Add(vector2Int.y);
                    break;
                case "Vector3":
                    Vector3Utility.TryParse(elementString, out var vector3);
                    components.Add(vector3.x);
                    components.Add(vector3.y);
                    components.Add(vector3.z);
                    break;
                case "Vector3Int":
                    Vector3IntUtility.TryParse(elementString, out var vector3Int);
                    components.Add(vector3Int.x);
                    components.Add(vector3Int.y);
                    components.Add(vector3Int.z);
                    break;
                case "Vector4":
                    Vector4Utility.TryParse(elementString, out var vector4);
                    components.Add(vector4.x);
                    components.Add(vector4.y);
                    components.Add(vector4.z);
                    components.Add(vector4.w);
                    break;
                case "RangeInt":
                    RangeIntUtility.TryParse(elementString, out var rangeInt);
                    components.Add(rangeInt.start);
                    components.Add(rangeInt.length);
                    break;
                case "Quaternion":
                    QuaternionUtility.TryParse(elementString, out var quaternion);
                    components.Add(quaternion.x);
                    components.Add(quaternion.y);
                    components.Add(quaternion.z);
                    components.Add(quaternion.w);
                    break;
                case "Rect":
                    RectUtility.TryParse(elementString, out var rect);
                    components.Add(rect.x);
                    components.Add(rect.y);
                    components.Add(rect.width);
                    components.Add(rect.height);
                    break;
                case "RectInt":
                    RectIntUtility.TryParse(elementString, out var rectInt);
                    components.Add(rectInt.x);
                    components.Add(rectInt.y);
                    components.Add(rectInt.width);
                    components.Add(rectInt.height);
                    break;
                case "Color":
                    ColorUtility.TryParse(elementString, out var color);
                    components.Add(color.r);
                    components.Add(color.g);
                    components.Add(color.b);
                    components.Add(color.a);
                    break;
                case "Color32":
                    Color32Utility.TryParse(elementString, out var color32);
                    components.Add(color32.r);
                    components.Add(color32.g);
                    components.Add(color32.b);
                    components.Add(color32.a);
                    break;
                default:
                    throw new NotSupportedException($"Unity type '{baseTypeName}' is not supported!");
            }
        }

        private static void AppendUnityArrayComponents(List<object> components, string baseTypeName, string value)
        {
            var separator = ArrayElementSeparator.ToString();
            switch (baseTypeName)
            {
                case "Vector2":
                    foreach (var element in Vector2Utility.ParseArray(value, separator))
                    {
                        components.Add(element.x);
                        components.Add(element.y);
                    }
                    break;
                case "Vector2Int":
                    foreach (var element in Vector2IntUtility.ParseArray(value, separator))
                    {
                        components.Add(element.x);
                        components.Add(element.y);
                    }
                    break;
                case "Vector3":
                    foreach (var element in Vector3Utility.ParseArray(value, separator))
                    {
                        components.Add(element.x);
                        components.Add(element.y);
                        components.Add(element.z);
                    }
                    break;
                case "Vector3Int":
                    foreach (var element in Vector3IntUtility.ParseArray(value, separator))
                    {
                        components.Add(element.x);
                        components.Add(element.y);
                        components.Add(element.z);
                    }
                    break;
                case "Vector4":
                    foreach (var element in Vector4Utility.ParseArray(value, separator))
                    {
                        components.Add(element.x);
                        components.Add(element.y);
                        components.Add(element.z);
                        components.Add(element.w);
                    }
                    break;
                case "RangeInt":
                    foreach (var element in RangeIntUtility.ParseArray(value, separator))
                    {
                        components.Add(element.start);
                        components.Add(element.length);
                    }
                    break;
                case "Quaternion":
                    foreach (var element in QuaternionUtility.ParseArray(value, separator))
                    {
                        components.Add(element.x);
                        components.Add(element.y);
                        components.Add(element.z);
                        components.Add(element.w);
                    }
                    break;
                case "Rect":
                    foreach (var element in RectUtility.ParseArray(value, separator))
                    {
                        components.Add(element.x);
                        components.Add(element.y);
                        components.Add(element.width);
                        components.Add(element.height);
                    }
                    break;
                case "RectInt":
                    foreach (var element in RectIntUtility.ParseArray(value, separator))
                    {
                        components.Add(element.x);
                        components.Add(element.y);
                        components.Add(element.width);
                        components.Add(element.height);
                    }
                    break;
                case "Color":
                    foreach (var element in ColorUtility.ParseArray(value, separator))
                    {
                        components.Add(element.r);
                        components.Add(element.g);
                        components.Add(element.b);
                        components.Add(element.a);
                    }
                    break;
                case "Color32":
                    foreach (var element in Color32Utility.ParseArray(value, separator))
                    {
                        components.Add(element.r);
                        components.Add(element.g);
                        components.Add(element.b);
                        components.Add(element.a);
                    }
                    break;
                default:
                    throw new NotSupportedException($"Unity type '{baseTypeName}' is not supported!");
            }
        }

        private static object ConvertPlainValue(Type targetType, string value)
        {
            // The string conversion replicates PropertyTypeConverter.Parse: empty strings are kept as
            // they are, non-empty strings are trimmed.
            if (targetType == typeof(string))
                return string.IsNullOrEmpty(value) ? value : value.Trim();

            var trimmed = value.Trim();

            if (targetType == typeof(bool))
            {
                if (string.IsNullOrEmpty(value))
                    return false;

                return bool.TryParse(CultureInfo.InvariantCulture.TextInfo.ToTitleCase(trimmed), out var result) && result;
            }

            if (targetType == typeof(byte))
                return byte.TryParse(trimmed, NumberStyles.Any, CultureInfo.InvariantCulture, out var resultByte) ? resultByte : (byte)0;

            if (targetType == typeof(sbyte))
                return sbyte.TryParse(trimmed, NumberStyles.Any, CultureInfo.InvariantCulture, out var resultSByte) ? resultSByte : (sbyte)0;

            if (targetType == typeof(short))
                return short.TryParse(trimmed, NumberStyles.Any, CultureInfo.InvariantCulture, out var resultInt16) ? resultInt16 : (short)0;

            if (targetType == typeof(ushort))
                return ushort.TryParse(trimmed, NumberStyles.Any, CultureInfo.InvariantCulture, out var resultUInt16) ? resultUInt16 : (ushort)0;

            if (targetType == typeof(int))
                return int.TryParse(trimmed, NumberStyles.Any, CultureInfo.InvariantCulture, out var resultInt32) ? resultInt32 : 0;

            if (targetType == typeof(uint))
                return uint.TryParse(trimmed, NumberStyles.Any, CultureInfo.InvariantCulture, out var resultUInt32) ? resultUInt32 : 0u;

            if (targetType == typeof(long))
                return long.TryParse(trimmed, NumberStyles.Any, CultureInfo.InvariantCulture, out var resultInt64) ? resultInt64 : 0L;

            if (targetType == typeof(ulong))
                return ulong.TryParse(trimmed, NumberStyles.Any, CultureInfo.InvariantCulture, out var resultUInt64) ? resultUInt64 : 0ul;

            if (targetType == typeof(float))
                return float.TryParse(trimmed, NumberStyles.Any, CultureInfo.InvariantCulture, out var resultSingle) ? resultSingle : 0f;

            if (targetType == typeof(double))
                return double.TryParse(trimmed, NumberStyles.Any, CultureInfo.InvariantCulture, out var resultDouble) ? resultDouble : 0d;

            if (targetType == typeof(decimal))
                return decimal.TryParse(trimmed, NumberStyles.Any, CultureInfo.InvariantCulture, out var resultDecimal) ? resultDecimal : 0m;

            throw new NotSupportedException($"Plain type '{targetType.Name}' is not supported!");
        }

        private static Type CreateEntityType(string entityTypeName, IList<ExpectedProperty> properties)
        {
            var assemblyName = new AssemblyName($"{typeof(Program).Namespace}.Entities.{Guid.NewGuid():N}");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name);
            var typeBuilder = moduleBuilder.DefineType(entityTypeName, TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed, typeof(MetadataEntity));

            foreach (var property in properties)
                AddAutoImplementedProperty(typeBuilder, property.PropertyName, property.StorageType);

            return typeBuilder.CreateType();
        }

        private static void AddAutoImplementedProperty(TypeBuilder typeBuilder, string propertyName, Type propertyType)
        {
            var fieldBuilder = typeBuilder.DefineField($"<{propertyName}>k__BackingField", propertyType, FieldAttributes.Private);
            var propertyBuilder = typeBuilder.DefineProperty(propertyName, System.Reflection.PropertyAttributes.None, propertyType, Type.EmptyTypes);

            var getterMethodBuilder = typeBuilder.DefineMethod($"get_{propertyName}",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                propertyType,
                Type.EmptyTypes);

            var ilGenerator = getterMethodBuilder.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldfld, fieldBuilder);
            ilGenerator.Emit(OpCodes.Ret);

            var setterMethodBuilder = typeBuilder.DefineMethod($"set_{propertyName}",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                typeof(void),
                [propertyType]);

            ilGenerator = setterMethodBuilder.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Stfld, fieldBuilder);
            ilGenerator.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getterMethodBuilder);
            propertyBuilder.SetSetMethod(setterMethodBuilder);
        }

        private static void VerifyEntity(ExpectedEntity model, IBoxDBAdapter adapter)
        {
            var entityType = model.EntityType;

            var count = Convert.ToInt64(adapter.Count(model.TableName));
            Check(count == model.Rows.Count, $"{model.EntityClassName}: Count is {model.Rows.Count} (actual: {count})");

            var entities = AdapterGetAllMethod.MakeGenericMethod(entityType).Invoke(adapter, [model.TableName]) as IEnumerable;
            var entityList = (entities ?? Array.Empty<object>()).Cast<object>().ToList();
            Check(entityList.Count == model.Rows.Count,
                $"{model.EntityClassName}: GetAll returns {model.Rows.Count} entities (actual: {entityList.Count})");

            var primaryKeyProperty = entityType.GetProperty(model.PrimaryKeyPropertyName);
            var entityMap = new Dictionary<object, object>();
            foreach (var entity in entityList)
                entityMap[primaryKeyProperty.GetValue(entity)] = entity;

            var verifiedCount = 0;
            var mismatchCount = 0;
            for (var i = 0; i < model.Rows.Count; i++)
            {
                var expectedRow = model.Rows[i];
                var rawRow = model.RawRows[i];
                var key = expectedRow[model.PrimaryKeyPropertyName];
                if (!entityMap.TryGetValue(key, out var entity))
                {
                    mismatchCount++;
                    PrintMismatch(model.EntityClassName, $"missing entity with {model.PrimaryKeyPropertyName} = {key}");
                    continue;
                }

                var rowMismatch = false;
                foreach (var property in model.Properties)
                {
                    var actual = entityType.GetProperty(property.PropertyName)?.GetValue(entity);
                    var expected = expectedRow[property.PropertyName];
                    if (!ValuesEqual(property, expected, actual))
                    {
                        rowMismatch = true;
                        PrintMismatch(model.EntityClassName,
                            $"{model.PrimaryKeyPropertyName} = {key}, {property.PropertyName} expected {FormatValue(expected)} but was {FormatValue(actual)}");
                        continue;
                    }

                    // Emphasize the Unity type data (and their arrays) and the enum type data with
                    // dedicated checks.
                    if (property.ValueKind == "Unity")
                    {
                        Check(true, $"{model.EntityClassName} row {i}: {DescribeUnityProperty(property, expected)} stored correctly");
                    }
                    else if (property.ValueKind == "Enum")
                    {
                        var enumIndex = (int)expected;
                        var rawCell = rawRow[property.PropertyName]?.Trim() ?? string.Empty;
                        var enumValueName = string.IsNullOrEmpty(rawCell) ? "None" : rawCell;
                        Check(enumIndex >= 0 && enumIndex < property.EnumValues.Count && property.EnumValues[enumIndex] == enumValueName,
                            $"{model.EntityClassName} row {i}: enum '{property.OriginalPropertyName}' value '{enumValueName}' stored as index {enumIndex} of {FormatEnumValues(property)}");
                    }
                }

                if (rowMismatch)
                    mismatchCount++;
                else
                    verifiedCount++;
            }

            Check(mismatchCount == 0, $"{model.EntityClassName}: {verifiedCount}/{model.Rows.Count} entities match the expected data");

            // Verify Get with the primary key of the first row.
            var firstRowKey = model.Rows[0][model.PrimaryKeyPropertyName];
            var firstEntity = AdapterGetMethod.MakeGenericMethod(entityType).Invoke(adapter, [model.TableName, new[] { firstRowKey }]);
            Check(firstEntity != null, $"{model.EntityClassName}: Get({FormatValue(firstRowKey)}) returns an entity");
        }

        private static byte[] UnwrapDatabaseFile(byte[] fileData)
        {
            using var reader = new BinaryReader(new MemoryStream(fileData));
            var encryptionFlag = reader.ReadByte();

            if (encryptionFlag > 0)
            {
                var key = reader.ReadBytes(CryptoProvider.EncryptionKeyLength);
                var compressionFlag = reader.ReadBoolean();
                var cipherData = reader.ReadBytes(fileData.Length - sizeof(byte) - CryptoProvider.EncryptionKeyLength - 1);
                var content = CryptoProvider.Decrypt(cipherData, key);
                return compressionFlag ? CompressionProvider.Decompress(content) : content;
            }

            var isCompressed = reader.ReadBoolean();
            var plainData = reader.ReadBytes(fileData.Length - sizeof(byte) - 1);
            return isCompressed ? CompressionProvider.Decompress(plainData) : plainData;
        }

        private static string GetRealTableName(string tableName) => tableName.Length > 32 ? Md5CryptoUtility.ComputeHashToHexString(tableName) : tableName;

        private static void PrintMismatch(string entityClassName, string message)
        {
            // Print the first 100 mismatches in detail to keep the output small.
            if (mismatchPrinted < 100)
            {
                mismatchPrinted++;
                Console.WriteLine($"[MISMATCH] {entityClassName}: {message}");
            }
        }

        private static bool ValuesEqual(ExpectedProperty property, object expected, object actual)
        {
            if (expected == null && actual == null)
                return true;

            if (expected == null || actual == null)
                return false;

            if (property.StorageType.IsArray)
            {
                var expectedArray = (Array)expected;
                if (!(actual is Array actualArray) || expectedArray.Length != actualArray.Length)
                    return false;

                for (var i = 0; i < expectedArray.Length; i++)
                {
                    if (!Equals(expectedArray.GetValue(i), actualArray.GetValue(i)))
                        return false;
                }

                return true;
            }

            return Equals(expected, actual);
        }

        private static string FormatValue(object value)
        {
            if (value == null)
                return "null";

            if (value is Array array)
            {
                var items = new List<string>();
                foreach (var item in array)
                    items.Add(FormatValue(item));

                return $"[{string.Join(", ", items)}]";
            }

            return Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        private static string DescribeUnityProperty(ExpectedProperty property, object expected)
        {
            var baseTypeName = property.UnityIsArray
                ? property.SourceTypeString[..^2]
                : property.SourceTypeString;
            return $"{baseTypeName} '{property.OriginalPropertyName}' stored as {FormatUnityValue(baseTypeName, property.UnityComponentCount, property.UnityIsArray, (Array)expected)}";
        }

        private static string FormatUnityValue(string baseTypeName, int componentCount, bool isArray, Array value)
        {
            var labels = UnityComponentNameMap[baseTypeName];
            var items = new List<string>();
            for (var i = 0; i + componentCount <= value.Length; i += componentCount)
            {
                var parts = new List<string>();
                for (var c = 0; c < componentCount; c++)
                    parts.Add($"{labels[c]}: {FormatValue(value.GetValue(i + c))}");

                items.Add($"({string.Join(", ", parts)})");
            }

            return isArray ? $"{baseTypeName}[{string.Join(", ", items)}]" : $"{baseTypeName}{items.FirstOrDefault() ?? "()"}";
        }

        private static string FormatEnumValues(ExpectedProperty property) => $"[{string.Join(", ", property.EnumValues)}]";

        private static string ToTitleCase(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            return char.ToUpperInvariant(value[0]) + value.Substring(1);
        }

        private static void ResetDirectory(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;
            
            if (Directory.Exists(path))
                Directory.Delete(path, true);

            Directory.CreateDirectory(path);
        }

        private static void PrintStep(string message) => Console.WriteLine($"{Environment.NewLine}=== {message} ===");

        private static void Check(bool condition, string message)
        {
            if (condition)
            {
                Console.WriteLine($"[PASS] {message}");
            }
            else
            {
                failureCount++;
                Console.WriteLine($"[FAIL] {message}");
            }
        }

        private static int PrintSummary()
        {
            Console.WriteLine();
            if (failureCount == 0)
            {
                Console.WriteLine("ALL TESTS PASSED!");
                return 0;
            }

            Console.WriteLine($"{failureCount} CHECK(S) FAILED!");
            return 1;
        }

        private sealed class ExpectedProperty
        {
            public string PropertyName;

            public string OriginalPropertyName;

            public string SourceTypeString;

            public string ValueKind;

            public Type StorageType;

            public int SourceColumn;

            public int UnityComponentCount;

            public bool UnityIsArray;

            public List<string> EnumValues;
        }

        private sealed class ExpectedEntity
        {
            public string EntityClassName;

            public Type EntityType;

            public string TableName;

            public string PrimaryKeyPropertyName;

            public List<ExpectedProperty> Properties;

            public List<Dictionary<string, object>> Rows;

            public List<Dictionary<string, string>> RawRows;
        }
    }
}
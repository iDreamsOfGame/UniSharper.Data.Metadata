// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ExcelDataReader;
using JetBrains.Annotations;
using ReSharp.Data.IBoxDB;
using ReSharp.Extensions;
using ReSharp.Security.Cryptography;
using UniSharper;
using UniSharper.Data.Metadata;
using UniSharper.Data.Metadata.Providers;
using UnityEditor;
using UnityEditor.Compilation;

// ReSharper disable RedundantArgumentDefaultValue

namespace UniSharperEditor.Data.Metadata
{
    internal static class MetadataAssetUtility
    {
        private const string UnityPackageName = "io.github.idreamsofgame.unisharper.data.metadata";

        private const string DatabaseAssetFileExtension = FileExtensions.DatabaseFile + FileExtensions.UnityBinaryAssetFile;

        private static string tempFolderPath;

        private static IDatabaseCryptoProvider databaseCryptoProvider;

        private static IDatabaseCompressionProvider databaseCompressionProvider;

        private static string TempFolderPath
        {
            get
            {
                if (string.IsNullOrEmpty(tempFolderPath))
                {
                    tempFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Temp", "UniSharper", "UniSharper.Data.Metadata", "Data");
                }

                if (!Directory.Exists(tempFolderPath))
                {
                    Directory.CreateDirectory(tempFolderPath);
                }

                return tempFolderPath;
            }
        }

        internal static bool CreateMetadataDatabaseFiles()
        {
            var result = false;
            var settings = MetadataAssetSettings.Load(true);
            MetadataAssetSettings.CreateMetadataPersistentStoreFolder(settings);
            DeleteTempDbFiles();

            FindChangedExcelWorkbookFiles(settings, out var addedExcelFiles, out var updatedExcelFiles, out var deletedExcelFiles);

            // Try to delete redundant metadata database file.
            TryDeleteMetadataDatabaseFiles(settings, deletedExcelFiles);

            var dbFolderPath = EditorPath.GetFullPath(settings.MetadataPersistentStorePath);
            var groups = LoadExcelFileGroups(MetadataAssetSettings.ExcelWorkbookFilesFolderPath);
            if (groups.Count == 0)
            {
                InternalEditorUtility.ClearProgressBar();
                return false;
            }

            foreach (var group in groups)
            {
                var info = $"Creating Database File for Entity {group.EntityName}... {group.Index + 1}/{groups.Count}";
                var progress = (float)(group.Index + 1) / groups.Count;
                EditorUtility.DisplayProgressBar("Hold on...", info, progress);

                var options = settings.CreateExcelDataParseOptions();
                var rawInfoList = CreateMetadataEntityRawInfoList(options, group.StructureFileName, group.Tables);
                var entityClassType = GetEntityClassType(settings, group.EntityName);

                if (entityClassType != null)
                {
                    var entityDataList = CreateEntityDataList(options, group.Tables, entityClassType, rawInfoList, group.StructureFileName);
                    var updateDatabaseFile = group.FileNames.Any(fileName => addedExcelFiles.Contains(fileName) || updatedExcelFiles.Contains(fileName));
                    typeof(MetadataAssetUtility).InvokeGenericStaticMethod("InsertEntityData",
                        new[] { entityClassType },
                        new object[] { settings, dbFolderPath, group.EntityName, rawInfoList, entityDataList, group.Index, updateDatabaseFile });
                    result = true;
                }
                else
                {
                    Debugger.LogError($"Can not find the entity class: {group.EntityName}{FileExtensions.CSharpScriptFile}!");
                    result = false;
                    break;
                }

                InternalEditorUtility.ClearProgressBar();
            }

            // Copy MetadataEntityDBConfig database file.
            if (result && (addedExcelFiles.Count > 0 || deletedExcelFiles.Count > 0))
            {
                CopyMetadataDatabaseFile(settings, dbFolderPath, MetadataEntityDBConfig.DatabaseLocalAddress);
            }

            InternalEditorUtility.ClearProgressBar();
            return result;
        }

        internal static bool GenerateMetadataEntityScripts(bool isCleanBuild = true)
        {
            var generated = false;
            var settings = MetadataAssetSettings.Load(true);
            MetadataAssetSettings.CreateEntityScriptsStoreFolder(settings);

            if (string.IsNullOrEmpty(MetadataAssetSettings.ExcelWorkbookFilesFolderPath))
            {
                if (EditorUtility.DisplayDialog("Error", "'Excel Workbook Files Folder Path' is not valid path!", "OK"))
                    InternalEditorUtility.ClearProgressBar();
            }
            else
            {
                FindChangedExcelWorkbookFiles(settings, out var addedExcelFiles, out var updatedExcelFiles, out var deletedExcelFiles);

                // Try to delete redundant entity scripts.
                TryDeleteMetadataEntityScripts(settings, deletedExcelFiles);

                var groups = LoadExcelFileGroups(MetadataAssetSettings.ExcelWorkbookFilesFolderPath);
                foreach (var group in groups)
                {
                    if (!isCleanBuild && !group.FileNames.Any(fileName => addedExcelFiles.Contains(fileName) || updatedExcelFiles.Contains(fileName)))
                        continue;

                    var info = $"Generating Metadata Entity Script: {group.EntityName}{FileExtensions.CSharpScriptFile}... {group.Index + 1}/{groups.Count}";
                    var progress = (float)(group.Index + 1) / groups.Count;
                    EditorUtility.DisplayProgressBar("Hold on...", info, progress);
                    var rawInfoList = CreateMetadataEntityRawInfoList(settings.CreateExcelDataParseOptions(), group.StructureFileName, group.Tables);
                    var success = GenerateMetadataEntityScript(settings, group.EntityName, rawInfoList);
                    if (success)
                    {
                        generated = true;
                    }
                    else
                    {
                        Debugger.LogError($"Generate metadata entity script failed: {group.EntityName}{FileExtensions.CSharpScriptFile}");
                    }
                    InternalEditorUtility.ClearProgressBar();
                }

                if (isCleanBuild)
                {
                    CompilationPipeline.RequestScriptCompilation(RequestScriptCompilationOptions.CleanBuildCache);
                }
                else
                {
                    CompilationPipeline.RequestScriptCompilation();
                }
            }

            InternalEditorUtility.ClearProgressBar();
            return generated;
        }

        internal static void SaveExcelWorkbookFilesSnapshot()
        {
            var paths = FileUtility.GetExcelFilePathCollection(MetadataAssetSettings.ExcelWorkbookFilesFolderPath);
            if (paths.Length == 0)
                return;

            var snapshot = new Dictionary<string, ExcelFileMetadata>();
            foreach (var path in paths)
            {
                var fileName = Path.GetFileName(path);
                var fileMetadata = ExcelFileMetadata.Create(path);
                snapshot.Add(fileName, fileMetadata);
            }

            ExcelFilesSnapshot.Save(snapshot);
        }

        internal static void ClearExcelWorkbookFilesSnapshot()
        {
            ExcelFilesSnapshot.Delete();
        }

        private static void FindChangedExcelWorkbookFiles(MetadataAssetSettings settings, 
            out List<string> addedExcelFiles, 
            out List<string> updatedExcelFiles, 
            out List<string> deletedExcelFiles)
        {
            addedExcelFiles = new List<string>();
            updatedExcelFiles = new List<string>();
            deletedExcelFiles = new List<string>();

            var oldSnapshot = ExcelFilesSnapshot.Load();
            var paths = FileUtility.GetExcelFilePathCollection(MetadataAssetSettings.ExcelWorkbookFilesFolderPath);
            if (paths.Length == 0)
            {
                if (oldSnapshot.Count > 0)
                    deletedExcelFiles = oldSnapshot.Keys.ToList();
                return;
            }

            var newSnapshot = new Dictionary<string, ExcelFileMetadata>();
            foreach (var path in paths)
            {
                var fileName = Path.GetFileName(path);
                var currentExcelFileMetadata = ExcelFileMetadata.Create(path);
                newSnapshot.Add(fileName, currentExcelFileMetadata);

                var entityScriptName = Path.GetFileNameWithoutExtension(path);
                var entityScriptFilePath = EditorPath.GetFullPath(settings.EntityScriptsStorePath, $"{entityScriptName}{FileExtensions.CSharpScriptFile}");
                if (!File.Exists(entityScriptFilePath))
                {
                    // New file.
                    addedExcelFiles.Add(fileName);
                    continue;
                }

                if (oldSnapshot.TryGetValue(fileName, out var oldExcelFileMetadata))
                {
                    // Updated file.
                    if (!oldExcelFileMetadata.Equals(currentExcelFileMetadata))
                        updatedExcelFiles.Add(fileName);
                }
                else
                {
                    // New file.
                    addedExcelFiles.Add(fileName);
                }
            }

            // Find deleted files.
            deletedExcelFiles.AddRange(oldSnapshot.Keys.Where(fileName => !newSnapshot.ContainsKey(fileName)));
        }

        private static void CopyMetadataDatabaseFile(MetadataAssetSettings settings,
            string dbFolderPath,
            long dbLocalAddress,
            string entityName = null)
        {
            var sourceFilePath = PathUtility.UnifyToAltDirectorySeparatorChar(Path.Combine(TempFolderPath, $"db{dbLocalAddress}.box"));
            var newFileName = dbLocalAddress > 1
                ? $"{entityName}{DatabaseAssetFileExtension}"
                : $"{nameof(MetadataEntityDBConfig)}{DatabaseAssetFileExtension}";
            var destFilePath = PathUtility.UnifyToAltDirectorySeparatorChar(Path.Combine(dbFolderPath, newFileName));
            FileUtil.ReplaceFile(sourceFilePath, destFilePath);
            PostProcessDatabaseFile(settings, destFilePath);
            var assetFilePath = EditorPath.GetAssetPath(destFilePath);
            AssetDatabase.ImportAsset(assetFilePath, ImportAssetOptions.ForceUpdate);
            SceneView.RepaintAll();
        }

        internal static List<MetadataEntity> CreateEntityDataList(ExcelDataParseOptions options,
            DataTable table,
            Type entityClassType,
            List<EntityPropertyRawInfo> rawInfoList)
        {
            return CreateEntityDataList(options, new List<DataTable> { table }, entityClassType, rawInfoList);
        }

        internal static List<MetadataEntity> CreateEntityDataList(ExcelDataParseOptions options,
            List<DataTable> tables,
            Type entityClassType,
            List<EntityPropertyRawInfo> rawInfoList,
            string fileName = null)
        {
            var list = new List<MetadataEntity>();
            if (tables == null || tables.Count == 0)
                return list;

            var firstTable = tables[0];

            for (var tableIndex = 0; tableIndex < tables.Count; tableIndex++)
            {
                var table = tables[tableIndex];
                if (table == null || table.Rows.Count <= options.EntityDataStartingRowIndex)
                    continue;

                // The first worksheet defines the data structure, so for the other worksheets the
                // columns are matched by the property name definition row.
                Dictionary<int, int> columnMap = null;
                if (tableIndex > 0)
                {
                    var worksheetDisplayName = GetWorksheetDisplayName(table, tableIndex);
                    columnMap = CreateColumnMapByPropertyName(options, firstTable, table, rawInfoList, fileName, worksheetDisplayName);
                    if (columnMap.Count == 0)
                        continue;
                }

                for (var i = options.EntityDataStartingRowIndex; i < table.Rows.Count; ++i)
                {
                    var entityData = (MetadataEntity)entityClassType.InvokeConstructor();
                    foreach (var rowInfo in rawInfoList)
                    {
                        var column = rowInfo.Column;
                        if (columnMap != null && !columnMap.TryGetValue(rowInfo.Column, out column))
                            continue;

                        var cellValue = table.Rows[i][column].ToString();
                        var typeParser = PropertyTypeConverterFactory.Instance.GetInstance(rowInfo.PropertyType);

                        if (typeParser != null)
                        {
                            var arrayElementSeparator = options.ArrayElementSeparator;
                            var value = typeParser.Parse(arrayElementSeparator, cellValue.Trim(), rowInfo.Parameters);
                            entityData.SetPropertyValue(rowInfo.PropertyName, value);
                        }
                        else
                        {
                            Debugger.LogWarning($"Type '{rowInfo.PropertyType}' is not supported!");
                        }
                    }

                    list.Add(entityData);
                }
            }

            return list;
        }

        private static Dictionary<int, int> CreateColumnMapByPropertyName(ExcelDataParseOptions options,
            DataTable firstTable,
            DataTable table,
            List<EntityPropertyRawInfo> rawInfoList,
            string fileName,
            string worksheetDisplayName)
        {
            var map = new Dictionary<int, int>();
            var nameRowIndex = options.EntityPropertyNameRowIndex;

            if (firstTable.Rows.Count <= nameRowIndex || table.Rows.Count <= nameRowIndex)
            {
                Debugger.LogWarning($"Worksheet '{worksheetDisplayName}' of Excel file '{fileName}' has no property name definition row, this worksheet will be ignored!");
                return map;
            }

            foreach (var rowInfo in rawInfoList)
            {
                var column = FindColumnByPropertyName(options, firstTable, table, rowInfo.Column);
                if (column >= 0)
                {
                    map.Add(rowInfo.Column, column);
                }
                else
                {
                    var propertyName = firstTable.Rows[nameRowIndex][rowInfo.Column].ToString().Trim();
                    Debugger.LogWarning($"Property '{propertyName}' not found in worksheet '{worksheetDisplayName}' of Excel file '{fileName}', its data will be ignored!");
                }
            }

            return map;
        }

        private static int FindColumnByPropertyName(ExcelDataParseOptions options, DataTable firstTable, DataTable table, int firstTableColumn)
        {
            var nameRowIndex = options.EntityPropertyNameRowIndex;
            if (firstTable.Rows.Count <= nameRowIndex || table.Rows.Count <= nameRowIndex)
                return -1;

            var propertyName = firstTable.Rows[nameRowIndex][firstTableColumn].ToString().Trim();
            if (string.IsNullOrEmpty(propertyName))
                return -1;

            for (var column = 0; column < table.Columns.Count; column++)
            {
                if (string.Equals(propertyName, table.Rows[nameRowIndex][column].ToString().Trim(), StringComparison.OrdinalIgnoreCase))
                    return column;
            }

            return -1;
        }

        private static string GetWorksheetDisplayName(DataTable table, int tableIndex) => string.IsNullOrEmpty(table.TableName) ? $"#{tableIndex + 1}" : table.TableName;

        internal static List<EntityPropertyRawInfo> CreateMetadataEntityRawInfoList(ExcelDataParseOptions options, string fileName, DataTable table)
        {
            var columns = table.Columns;
            var rows = table.Rows;
            var columnCount = columns.Count;
            var list = new List<EntityPropertyRawInfo>();

            for (var column = 0; column < columnCount; column++)
            {
                var propertyName = rows[options.EntityPropertyNameRowIndex][column].ToString();
                var propertyType = rows[options.EntityPropertyTypeRowIndex][column].ToString();
                var comment = rows[options.EntityPropertyCommentRowIndex][column].ToString();

                if (string.IsNullOrEmpty(propertyName))
                {
                    var message = $"Invalid property name at column index {column}, column data will be ignored!";
                    Debugger.Log(message);
                    continue;
                }

                if (string.IsNullOrEmpty(propertyType))
                {
                    Debugger.Log(string.IsNullOrEmpty(propertyName)
                        ? $"Invalid property type at column index {column}, column data will be ignored in file '{fileName}'!"
                        : $"Invalid property type with name '{propertyName}' at column index {column}, column data will be ignored in file '{fileName}'!");

                    continue;
                }

                propertyName = propertyName.Trim().ToTitleCase();
                propertyType = propertyType.Trim();
                comment = FormatCommentString(comment.Trim());

                var editor = PropertyRawInfoEditorFactory.Instance.GetInstance(propertyType);
                var rawInfo = editor?.Edit(options, table, column, comment, propertyType, propertyName)
                              ?? new EntityPropertyRawInfo(column, comment, propertyType, propertyName);
                list.Add(rawInfo);
            }

            return list;
        }

        internal static List<EntityPropertyRawInfo> CreateMetadataEntityRawInfoList(ExcelDataParseOptions options, string fileName, List<DataTable> tables)
        {
            // The first worksheet defines the data structure (property comment, type, name and data
            // starting rows), the other worksheets only provide entity data rows.
            var rawInfoList = tables is { Count: > 0 } ? CreateMetadataEntityRawInfoList(options, fileName, tables[0]) : new List<EntityPropertyRawInfo>();
            if (rawInfoList.Count > 0)
                AppendEnumValuesFromOtherWorksheets(options, tables, rawInfoList);

            return rawInfoList;
        }

        private static void AppendEnumValuesFromOtherWorksheets(ExcelDataParseOptions options,
            List<DataTable> tables,
            List<EntityPropertyRawInfo> rawInfoList)
        {
            var firstTable = tables[0];

            foreach (var rawInfo in rawInfoList.Where(rowInfo => rowInfo.PropertyType.Equals(PropertyTypeNames.Enum)))
            {
                var enumValues = (rawInfo.Parameters[2] as string[])?.ToList();
                if (enumValues == null)
                    continue;

                for (var tableIndex = 1; tableIndex < tables.Count; tableIndex++)
                {
                    var table = tables[tableIndex];
                    if (table == null || table.Rows.Count <= options.EntityDataStartingRowIndex)
                        continue;

                    var column = FindColumnByPropertyName(options, firstTable, table, rawInfo.Column);
                    if (column < 0)
                        continue;

                    for (var row = options.EntityDataStartingRowIndex; row < table.Rows.Count; row++)
                    {
                        var enumValue = table.Rows[row][column].ToString().Trim();

                        if (!string.IsNullOrEmpty(enumValue) && !enumValues.Contains(enumValue))
                            enumValues.Add(enumValue);
                    }
                }

                rawInfo.Parameters[2] = enumValues.ToArray();
            }
        }

        private static void DeleteTempDbFiles()
        {
            FileUtil.DeleteFileOrDirectory(TempFolderPath);
        }

        private static void PostProcessDatabaseFile(MetadataAssetSettings settings, string filePath)
        {
            var rawData = File.ReadAllBytes(filePath);
            var encryptionFlag = (byte)(settings.ShouldEncryptDatabase ? 1 : 0);
            var compressionFlag = (byte)(settings.ShouldCompressDatabase ? 1 : 0);
            var cryptoProvider = GetDatabaseCryptoProvider(settings);
            var compressionProvider = GetDatabaseCompressionProvider(settings);

            using var writer = new BinaryWriter(new MemoryStream());
            writer.Write(encryptionFlag);

            if (settings.ShouldEncryptDatabase)
            {
                var key = CryptoUtility.GenerateRandomKey(cryptoProvider.EncryptionKeyLength);
                var output = settings.ShouldCompressDatabase ? compressionProvider.Compress(rawData) : rawData;
                var cipherData = cryptoProvider.Encrypt(output, key);
                writer.Write(key);
                writer.Write(compressionFlag);
                writer.Write(cipherData);
            }
            else
            {
                var output = settings.ShouldCompressDatabase ? compressionProvider.Compress(rawData) : rawData;
                writer.Write(compressionFlag);
                writer.Write(output);
            }

            var bufferData = ((MemoryStream)writer.BaseStream).GetBuffer();
            File.WriteAllBytes(filePath, bufferData);
        }

        internal sealed class ExcelFileGroup
        {
            public string EntityName { get; set; }

            public string StructureFileName { get; set; }

            public int Index { get; set; }

            public List<string> FileNames { get; } = new List<string>();

            public List<DataTable> Tables { get; } = new List<DataTable>();
        }

        private static string GetEntityName(string fileNameWithoutExtension) => fileNameWithoutExtension.Split('-')[0].Trim().ToTitleCase();

        private static HashSet<string> GetEntityNamesOfExcelFiles()
        {
            var entityNames = new HashSet<string>();
            var paths = FileUtility.GetExcelFilePathCollection(MetadataAssetSettings.ExcelWorkbookFilesFolderPath);
            foreach (var path in paths)
            {
                entityNames.Add(GetEntityName(Path.GetFileNameWithoutExtension(path)));
            }

            return entityNames;
        }

        internal static List<ExcelFileGroup> LoadExcelFileGroups(string excelFilesFolderPath)
        {
            var paths = FileUtility.GetExcelFilePathCollection(excelFilesFolderPath);
            if (paths.Length == 0)
            {
                Debugger.LogError("No excel files found!");
                return new List<ExcelFileGroup>();
            }

            return LoadExcelFileGroups(paths);
        }

        internal static List<ExcelFileGroup> LoadExcelFileGroups(IList<string> excelFilePaths)
        {
            var groups = new List<ExcelFileGroup>();

            // Sort the file paths to keep the merging order and the structure source deterministic.
            var paths = new string[excelFilePaths.Count];
            excelFilePaths.CopyTo(paths, 0);
            Array.Sort(paths, StringComparer.Ordinal);

            for (var i = 0; i < paths.Length; i++)
            {
                var path = paths[i];
                var fileName = Path.GetFileName(path);

                try
                {
                    var tables = ReadExcelFileDataTables(path);
                    if (tables is not { Count: > 0 })
                    {
                        Debugger.LogError($"Excel file '{fileName}' should have a table at least!");
                        continue;
                    }

                    // Excel files whose name contains a hyphen '-' are merged into one database named
                    // by the string before the first hyphen, e.g. 'AAMetadata-Module1' and
                    // 'AAMetadata-Module2' are merged into the database 'AAMetadata'.
                    var entityName = GetEntityName(Path.GetFileNameWithoutExtension(fileName));
                    var group = groups.Find(candidate => candidate.EntityName.Equals(entityName));
                    if (group == null)
                    {
                        group = new ExcelFileGroup { EntityName = entityName, StructureFileName = fileName, Index = groups.Count };
                        groups.Add(group);
                    }

                    group.FileNames.Add(fileName);
                    group.Tables.AddRange(tables);
                }
                catch (Exception e)
                {
                    Debugger.LogError($"fileName: {fileName}, error: {e}");
                    InternalEditorUtility.ClearProgressBar();
                    throw;
                }
            }

            return groups;
        }

        internal static DataTable ReadExcelFileDataTable(string filePath)
        {
            var tables = ReadExcelFileDataTables(filePath);
            return tables is { Count: > 0 } ? tables[0] : null;
        }

        internal static List<DataTable> ReadExcelFileDataTables(string filePath)
        {
            using var stream = File.OpenRead(filePath);
            using var reader = ExcelReaderFactory.CreateReader(stream);
            var result = reader.AsDataSet();
            if (result is not { Tables: { Count: > 0 } })
                return null;

            // Empty worksheets and worksheets whose name starts with "##" are ignored.
            var tables = new List<DataTable>(result.Tables.Count);
            tables.AddRange(result.Tables.Cast<DataTable>()
                .Where(table => table != null && !IsEmptyWorksheet(table) && !IsIgnoredWorksheetName(table.TableName)));

            return tables.Count > 0 ? tables : null;
        }

        private static bool IsEmptyWorksheet(DataTable table) => table.Columns.Count == 0 || table.Rows.Count == 0;

        private static bool IsIgnoredWorksheetName(string worksheetName) => !string.IsNullOrEmpty(worksheetName) && worksheetName.StartsWith("##", StringComparison.Ordinal);

        private static string FormatCommentString(string comment)
        {
            if (string.IsNullOrEmpty(comment))
                return comment;

            const string pattern = @"\r*\n";
            var regex = new Regex(pattern);
            return regex.Replace(comment, PlayerEnvironment.WindowsNewLine + "\t\t/// ");
        }

        private static string GenerateEntityScriptEnumString(List<EntityPropertyRawInfo> rawInfoList)
        {
            var stringBuilder = new StringBuilder();

            foreach (var rawInfo in rawInfoList.Where(rowInfo => rowInfo.PropertyType.Equals(PropertyTypeNames.Enum)))
            {
                var enumValuesString = new StringBuilder();
                var enumValueList = rawInfo.Parameters[2] as string[];
                if (enumValueList == null)
                    continue;

                for (var j = 0; j < enumValueList.Length; j++)
                {
                    var enumValue = enumValueList[j];
                    enumValuesString.Append($"\t\t\t{enumValue}");

                    if (j >= enumValueList.Length - 1)
                        continue;

                    enumValuesString.Append(",").Append(PlayerEnvironment.WindowsNewLine);
                }

                if (stringBuilder.Length > 0)
                    stringBuilder.Append(PlayerEnvironment.WindowsNewLine)
                        .Append(PlayerEnvironment.WindowsNewLine);

                stringBuilder.AppendFormat(ScriptTemplate.ClassMemberFormatString.EnumDefinition, rawInfo.Comment, rawInfo.Parameters[1], enumValuesString);
            }

            if (stringBuilder.Length > 0)
                stringBuilder.Append(PlayerEnvironment.WindowsNewLine);

            return stringBuilder.ToString();
        }

        private static string GenerateEntityScriptFieldsString(List<EntityPropertyRawInfo> rawInfoList)
        {
            var stringBuilder = new StringBuilder();

            foreach (var rawInfo in rawInfoList.Where(rowInfo => PropertyTypeNames.IsUnityTypeArray(rowInfo.PropertyType)))
            {
                if (stringBuilder.Length > 0)
                    stringBuilder.Append(PlayerEnvironment.WindowsNewLine)
                        .Append(PlayerEnvironment.WindowsNewLine);

                var fieldType = rawInfo.PropertyType;
                var fieldName = (rawInfo.Parameters[1] as string).ToCamelCase();
                stringBuilder.AppendFormat(ScriptTemplate.ClassMemberFormatString.PrivateField, fieldType, fieldName);
            }

            if (stringBuilder.Length > 0)
                stringBuilder.Append(PlayerEnvironment.WindowsNewLine);

            return stringBuilder.ToString();
        }

        private static string GenerateEntityScriptPropertiesString(List<EntityPropertyRawInfo> rawInfoList)
        {
            var stringBuilder = new StringBuilder();

            for (int i = 0, length = rawInfoList.Count; i < length; ++i)
            {
                var rawInfo = rawInfoList[i];
                var propertyType = rawInfo.PropertyType;
                var editor = PropertyStringEditorFactory.Instance.GetInstance(propertyType);
                if (editor != null)
                {
                    editor.Edit(stringBuilder, rawInfo);
                }
                else
                {
                    stringBuilder.AppendFormat(ScriptTemplate.ClassMemberFormatString.AutoImplementedProperty, rawInfo.Comment, propertyType, rawInfo.PropertyName);
                }

                if (i >= length - 1)
                    continue;

                stringBuilder.Append(PlayerEnvironment.WindowsNewLine)
                    .Append(PlayerEnvironment.WindowsNewLine);
            }

            return stringBuilder.ToString();
        }

        private static bool GenerateMetadataEntityScript(MetadataAssetSettings settings, string entityScriptName, List<EntityPropertyRawInfo> rawInfoList)
        {
            try
            {
                entityScriptName = entityScriptName.ToTitleCase();
                const string templateFileName = "NewMetadataEntityScriptTemplate.txt";
                var newLineStringLength = PlayerEnvironment.WindowsNewLine.Length;
                var scriptTextContent = ScriptTemplate.LoadScriptTemplateFile(templateFileName, UnityPackageName);
                scriptTextContent = scriptTextContent.Replace(ScriptTemplate.Placeholders.Namespace, settings.EntityScriptNamespace);
                scriptTextContent = scriptTextContent.Replace(ScriptTemplate.Placeholders.ScriptName, entityScriptName);

                // Enum string
                var enumString = GenerateEntityScriptEnumString(rawInfoList);
                var startIndex = scriptTextContent.IndexOf(ScriptTemplate.Placeholders.EnumInsideOfClass, StringComparison.Ordinal);
                if (string.IsNullOrEmpty(enumString))
                    scriptTextContent = scriptTextContent.Remove(startIndex - newLineStringLength, newLineStringLength);

                scriptTextContent = scriptTextContent.Replace(ScriptTemplate.Placeholders.EnumInsideOfClass, enumString);

                // Fields string
                var fieldsString = GenerateEntityScriptFieldsString(rawInfoList);
                startIndex = scriptTextContent.IndexOf(ScriptTemplate.Placeholders.Fields, StringComparison.Ordinal);
                if (string.IsNullOrEmpty(fieldsString))
                    scriptTextContent = scriptTextContent.Remove(startIndex - newLineStringLength, newLineStringLength);

                scriptTextContent = scriptTextContent.Replace(ScriptTemplate.Placeholders.Fields, fieldsString);

                scriptTextContent = scriptTextContent.Replace(ScriptTemplate.Placeholders.Properties, GenerateEntityScriptPropertiesString(rawInfoList));

                var scriptFilePath = EditorPath.GetFullPath(settings.EntityScriptsStorePath, $"{entityScriptName}{FileExtensions.CSharpScriptFile}");
                var scriptAssetPath = EditorPath.GetAssetPath(scriptFilePath);
                File.WriteAllText(scriptFilePath, scriptTextContent, new UTF8Encoding(true));
                AssetDatabase.ImportAsset(scriptAssetPath);
                return true;
            }
            catch (Exception exception)
            {
                Debugger.LogError(exception.ToString());
                return false;
            }
        }

        private static Type GetEntityClassType(MetadataAssetSettings settings, string entityClassName)
        {
            var scriptAssetPath = PathUtility.UnifyToAltDirectorySeparatorChar(Path.Combine(settings.EntityScriptsStorePath, $"{entityClassName}.cs"));
            var scriptObject = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptAssetPath);
            return scriptObject ? scriptObject.GetClass() : null;
        }

        [UsedImplicitly]
        private static void InsertEntityData<T>(MetadataAssetSettings settings,
            string dbFolderPath,
            string tableName,
            IList<EntityPropertyRawInfo> rowInfos,
            IList<MetadataEntity> entityDataList,
            int index,
            bool updateDatabaseFile = true) where T : MetadataEntity
        {
            var dbLocalAddress = index + 2L;
            const string dbName = nameof(MetadataEntityDBConfig);

            using var dbAdapter = new IBoxDBAdapter(TempFolderPath, MetadataEntityDBConfig.DatabaseLocalAddress);
            dbAdapter.EnsureTable<MetadataEntityDBConfig>(dbName, MetadataEntityDBConfig.TablePrimaryKey);
            dbAdapter.Open();
            var tablePrimaryKey = rowInfos[0].PropertyName;
            var entityName = MetadataManager.GetRealTableName(tableName);
            var success = dbAdapter.Insert(new MetadataEntityDBConfig(entityName, tablePrimaryKey));
            if (!success)
                return;

            if (!updateDatabaseFile)
                return;

            var dataList = new T[entityDataList.Count];

            for (var i = 0; i < entityDataList.Count; i++)
            {
                dataList[i] = (T)entityDataList[i];
            }

            using var dataDBAdapter = new IBoxDBAdapter(TempFolderPath, dbLocalAddress);
            dataDBAdapter.EnsureTable<T>(entityName, tablePrimaryKey);
            dataDBAdapter.Open();

            foreach (var data in dataList)
            {
                var result = dataDBAdapter.Insert(entityName, data);

                if (!result)
                    Debugger.LogWarning($"Can not write metadata ({data}) into database file: {tableName}!");
            }

            // Copy metadata entity database file.
            CopyMetadataDatabaseFile(settings, dbFolderPath, dbLocalAddress, tableName);
        }

        private static void TryDeleteMetadataEntityScripts(MetadataAssetSettings settings, List<string> deletedExcelFiles)
        {
            if (settings.ShouldRemoveRedundantFiles && deletedExcelFiles.Count > 0)
            {
                var remainingEntityNames = GetEntityNamesOfExcelFiles();
                foreach (var deletedExcelFile in deletedExcelFiles)
                {
                    var entityName = GetEntityName(Path.GetFileNameWithoutExtension(deletedExcelFile));
                    if (remainingEntityNames.Contains(entityName))
                        continue;

                    DeleteMetadataEntityScript(settings, entityName);
                }

                AssetDatabase.Refresh();
                SceneView.RepaintAll();
            }
        }

        private static void DeleteMetadataEntityScript(MetadataAssetSettings settings, string entityName)
        {
            try
            {
                var scriptFilePath = Path.Combine(settings.EntityScriptsStorePath, $"{entityName}{FileExtensions.CSharpScriptFile}");
                if (AssetDatabase.DeleteAsset(scriptFilePath))
                    return;

                // Plan B: Try to delete file by using class FileUtil.
                var scriptMetaFilePath = $"{scriptFilePath}{FileExtensions.UnityMetaFile}";
                FileUtil.DeleteFileOrDirectory(scriptFilePath);
                FileUtil.DeleteFileOrDirectory(scriptMetaFilePath);
            }
            catch (Exception e)
            {
                Debugger.LogError(e.ToString());
            }
        }

        private static void TryDeleteMetadataDatabaseFiles(MetadataAssetSettings settings, List<string> deletedExcelFiles)
        {
            if (settings.ShouldRemoveRedundantFiles && deletedExcelFiles.Count > 0)
            {
                var remainingEntityNames = GetEntityNamesOfExcelFiles();
                foreach (var deletedExcelFile in deletedExcelFiles)
                {
                    var entityName = GetEntityName(Path.GetFileNameWithoutExtension(deletedExcelFile));
                    if (remainingEntityNames.Contains(entityName))
                        continue;

                    DeleteMetadataDatabaseFile(settings, entityName);
                }

                AssetDatabase.Refresh();
            }
        }

        private static void DeleteMetadataDatabaseFile(MetadataAssetSettings settings, string entityName)
        {
            var databaseFilePath = Path.Combine(settings.MetadataPersistentStorePath, $"{entityName}{FileExtensions.DatabaseFile}");
            if (AssetDatabase.DeleteAsset(databaseFilePath))
                return;

            // Plan B: Try to delete file by using class FileUtil.
            databaseFilePath = $"{databaseFilePath}{FileExtensions.UnityBinaryAssetFile}";
            var scriptMetaFilePath = $"{databaseFilePath}{FileExtensions.UnityMetaFile}";
            FileUtil.DeleteFileOrDirectory(databaseFilePath);
            FileUtil.DeleteFileOrDirectory(scriptMetaFilePath);
        }

        private static IDatabaseCryptoProvider GetDatabaseCryptoProvider(MetadataAssetSettings settings)
        {
            if (databaseCryptoProvider != null)
                return databaseCryptoProvider;

            var type = Type.GetType(settings.DatabaseCryptoProvider);
            if (type == null)
                return null;

            databaseCryptoProvider = Activator.CreateInstance(type) as IDatabaseCryptoProvider;
            return databaseCryptoProvider;
        }

        private static IDatabaseCompressionProvider GetDatabaseCompressionProvider(MetadataAssetSettings settings)
        {
            if (databaseCompressionProvider != null)
                return databaseCompressionProvider;

            var type = Type.GetType(settings.DatabaseCompressionProvider);
            if (type == null)
                return null;

            databaseCompressionProvider = Activator.CreateInstance(type) as IDatabaseCompressionProvider;
            return databaseCompressionProvider;
        }
    }
}
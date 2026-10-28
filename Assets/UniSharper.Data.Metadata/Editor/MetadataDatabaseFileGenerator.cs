// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using ReSharp.Data.IBoxDB;
using ReSharp.Extensions;
using ReSharp.Security.Cryptography;
using UniSharper;
using UniSharper.Data.Metadata;
using UniSharper.Data.Metadata.Providers;
using UniSharperEditor.Data.Metadata.PropertyRawInfoEditors;

namespace UniSharperEditor.Data.Metadata
{
    /// <summary>
    /// Generates metadata database files from Excel workbook files (*.xls, *.xlsx) directly, without
    /// generating the metadata entity scripts. The metadata entity types used to write data into
    /// database files are created dynamically by using C# Reflection.Emit.
    /// </summary>
    public static class MetadataDatabaseFileGenerator
    {
        private const string DatabaseFileExtension = FileExtensions.DatabaseFile + FileExtensions.UnityBinaryAssetFile;

        private const string TempFolderName = "Generator";

        private static readonly MethodInfo EnsureTableMethod = Array.Find(typeof(IBoxDBAdapter).GetMethods(BindingFlags.Public | BindingFlags.Instance),
            method => method.Name == nameof(IBoxDBAdapter.EnsureTable)
                      && method.IsGenericMethodDefinition
                      && method.GetParameters().Length == 2
                      && method.GetParameters()[0].ParameterType == typeof(string)
                      && method.GetParameters()[1].ParameterType == typeof(string));

        private static readonly MethodInfo InsertMethod = Array.Find(typeof(IBoxDBAdapter).GetMethods(BindingFlags.Public | BindingFlags.Instance),
            method => method.Name == nameof(IBoxDBAdapter.Insert)
                      && method.IsGenericMethodDefinition
                      && method.GetParameters().Length == 2
                      && method.GetParameters()[0].ParameterType == typeof(string)
                      && method.GetParameters()[1].ParameterType.IsGenericParameter);

        private static readonly Dictionary<string, Type> PropertyTypeMap = new()
        {
            { PropertyTypeNames.String, typeof(string) },
            { PropertyTypeNames.Boolean, typeof(bool) },
            { PropertyTypeNames.Byte, typeof(byte) },
            { PropertyTypeNames.SByte, typeof(sbyte) },
            { PropertyTypeNames.Decimal, typeof(decimal) },
            { PropertyTypeNames.Double, typeof(double) },
            { PropertyTypeNames.Single, typeof(float) },
            { PropertyTypeNames.Int32, typeof(int) },
            { PropertyTypeNames.UInt32, typeof(uint) },
            { PropertyTypeNames.Int64, typeof(long) },
            { PropertyTypeNames.UInt64, typeof(ulong) },
            { PropertyTypeNames.Int16, typeof(short) },
            { PropertyTypeNames.UInt16, typeof(ushort) }
        };

        private static string tempFolderPath;

        /// <summary>
        /// Generates a metadata database file from an Excel workbook file (*.xls, *.xlsx). The first
        /// worksheet of the Excel workbook file is used as the metadata table, also the configuration
        /// database file (<see cref="MetadataEntityDBConfig"/>) will be generated for the metadata entity.
        /// </summary>
        /// <param name="excelFilePath">The path of the Excel workbook file (*.xls, *.xlsx).</param>
        /// <param name="saveFolderPath">The folder path where to save the database files.</param>
        /// <param name="options">The Excel data parse options.</param>
        /// <param name="shouldEncryptDatabase">Encrypt database or not.</param>
        /// <param name="shouldCompressDatabase">Compress database or not.</param>
        /// <param name="cryptoProvider">The crypto provider instance to encrypt database data. If null, the default crypto provider (<see cref="AesCbcCryptoProvider"/>) will be used.</param>
        /// <param name="compressionProvider">The compression provider instance to compress database data. If null, the default compression provider (<see cref="DeflateCompressionProvider"/>) will be used.</param>
        /// <returns><c>true</c> if the database file generated successfully; otherwise, <c>false</c>.</returns>
        public static bool GenerateDatabaseFile(string excelFilePath,
            string saveFolderPath,
            ExcelDataParseOptions options,
            bool shouldEncryptDatabase,
            bool shouldCompressDatabase,
            IDatabaseCryptoProvider cryptoProvider = null,
            IDatabaseCompressionProvider compressionProvider = null)
        {
            if (string.IsNullOrEmpty(excelFilePath) || !File.Exists(excelFilePath))
            {
                LogError("The excel file path is null, empty or the file does not exist!");
                return false;
            }

            var extension = Path.GetExtension(excelFilePath);
            if (!extension.Equals(FileExtensions.ExcelSpreadsheetFile, StringComparison.OrdinalIgnoreCase)
                && !extension.Equals(FileExtensions.ExcelWorkbookFile, StringComparison.OrdinalIgnoreCase))
            {
                LogError($"The excel file '{excelFilePath}' is not a valid excel file, only supports '{FileExtensions.ExcelSpreadsheetFile}' and '{FileExtensions.ExcelWorkbookFile}' files!");
                return false;
            }

            if (string.IsNullOrEmpty(saveFolderPath))
            {
                LogError("The database files save folder path can not be null or empty!");
                return false;
            }

            return GenerateDatabaseFiles(new[] { excelFilePath },
                saveFolderPath,
                options,
                shouldEncryptDatabase,
                shouldCompressDatabase,
                cryptoProvider,
                compressionProvider);
        }

        /// <summary>
        /// Generates metadata database files from all Excel workbook files (*.xls, *.xlsx) in the
        /// specified folder. The first worksheet of each Excel workbook file is used as the metadata
        /// table, also the configuration database file (<see cref="MetadataEntityDBConfig"/>) will be
        /// generated for all metadata entities.
        /// </summary>
        /// <param name="excelFilesFolderPath">The folder path where to locate excel workbook files (*.xls, *.xlsx).</param>
        /// <param name="saveFolderPath">The folder path where to save the database files.</param>
        /// <param name="options">The Excel data parse options.</param>
        /// <param name="shouldEncryptDatabase">Encrypt database or not.</param>
        /// <param name="shouldCompressDatabase">Compress database or not.</param>
        /// <param name="cryptoProvider">The crypto provider instance to encrypt database data. If null, the default crypto provider (<see cref="AesCbcCryptoProvider"/>) will be used.</param>
        /// <param name="compressionProvider">The compression provider instance to compress database data. If null, the default compression provider (<see cref="DeflateCompressionProvider"/>) will be used.</param>
        /// <returns><c>true</c> if all database files generated successfully; otherwise, <c>false</c>.</returns>
        public static bool GenerateDatabaseFiles(string excelFilesFolderPath,
            string saveFolderPath,
            ExcelDataParseOptions options,
            bool shouldEncryptDatabase,
            bool shouldCompressDatabase,
            IDatabaseCryptoProvider cryptoProvider = null,
            IDatabaseCompressionProvider compressionProvider = null)
        {
            if (string.IsNullOrEmpty(excelFilesFolderPath) || !Directory.Exists(excelFilesFolderPath))
            {
                LogError("The excel files folder path is null, empty or the folder does not exist!");
                return false;
            }

            if (string.IsNullOrEmpty(saveFolderPath))
            {
                LogError("The database files save folder path can not be null or empty!");
                return false;
            }

            var excelFilePaths = FileUtility.GetExcelFilePathCollection(excelFilesFolderPath);
            if (excelFilePaths.Length == 0)
            {
                LogError("No excel files found!");
                return false;
            }

            return GenerateDatabaseFiles(excelFilePaths,
                saveFolderPath,
                options,
                shouldEncryptDatabase,
                shouldCompressDatabase,
                cryptoProvider,
                compressionProvider);
        }

        private static bool GenerateDatabaseFiles(IList<string> excelFilePaths,
            string saveFolderPath,
            ExcelDataParseOptions options,
            bool shouldEncryptDatabase,
            bool shouldCompressDatabase,
            IDatabaseCryptoProvider cryptoProvider,
            IDatabaseCompressionProvider compressionProvider)
        {
            cryptoProvider ??= new AesCbcCryptoProvider();
            compressionProvider ??= new DeflateCompressionProvider();
            
            var result = false;
            var hasError = false;
            ResetTempFolder();

            if (!Directory.Exists(saveFolderPath))
                Directory.CreateDirectory(saveFolderPath);

            var assemblyName = new AssemblyName($"{typeof(MetadataDatabaseFileGenerator).FullName}.{Guid.NewGuid():N}");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name);
            var generatedEntityNames = new HashSet<string>();

            try
            {
                for (var i = 0; i < excelFilePaths.Count; i++)
                {
                    var excelFilePath = excelFilePaths[i];
                    var fileName = Path.GetFileName(excelFilePath);
                    var entityClassName = Path.GetFileNameWithoutExtension(fileName).ToTitleCase();
                    LogInfo($"Creating Database File for Entity {entityClassName}... {i + 1}/{excelFilePaths.Count}");

                    try
                    {
                        var table = MetadataAssetUtility.ReadExcelFileDataTable(excelFilePath);
                        if (table == null)
                        {
                            LogError($"Excel file '{fileName}' should have a table at least!");
                            hasError = true;
                            continue;
                        }

                        if (!generatedEntityNames.Add(entityClassName))
                        {
                            LogError($"Excel file '{fileName}' has a duplicated entity name '{entityClassName}', it will be ignored!");
                            hasError = true;
                            continue;
                        }

                        var rawInfoList = MetadataAssetUtility.CreateMetadataEntityRawInfoList(options, fileName, table);
                        if (rawInfoList.Count == 0)
                        {
                            LogError($"No valid property definitions found in excel file '{fileName}'!");
                            hasError = true;
                            continue;
                        }

                        var entityType = CreateEntityType(moduleBuilder, entityClassName, rawInfoList);
                        var entityDataList = MetadataAssetUtility.CreateEntityDataList(options, table, entityType, rawInfoList);
                        var dbLocalAddress = i + 2L;
                        GenerateEntityDatabaseFile(entityClassName,
                            rawInfoList,
                            entityDataList,
                            entityType,
                            saveFolderPath,
                            dbLocalAddress,
                            shouldEncryptDatabase,
                            shouldCompressDatabase,
                            cryptoProvider,
                            compressionProvider);

                        result = true;
                    }
                    catch (Exception e)
                    {
                        LogError($"Create database file for entity '{entityClassName}' failed, fileName: {fileName}, error: {e}");
                        hasError = true;
                    }
                }

                // Copy MetadataEntityDBConfig database file.
                if (result)
                    SaveConfigDatabaseFile(saveFolderPath, shouldEncryptDatabase, shouldCompressDatabase, cryptoProvider, compressionProvider);
            }
            finally
            {
                DeleteTempFolder();
            }

            return result && !hasError;
        }

        private static void GenerateEntityDatabaseFile(string entityClassName,
            IList<EntityPropertyRawInfo> rawInfoList,
            IList<MetadataEntity> entityDataList,
            Type entityType,
            string saveFolderPath,
            long dbLocalAddress,
            bool shouldEncryptDatabase,
            bool shouldCompressDatabase,
            IDatabaseCryptoProvider cryptoProvider,
            IDatabaseCompressionProvider compressionProvider)
        {
            var tableName = MetadataManager.GetRealTableName(entityClassName);
            var tablePrimaryKey = rawInfoList[0].PropertyName;

            // Write the entity config record into the config database.
            using (var configDBAdapter = new IBoxDBAdapter(TempFolderPath, MetadataEntityDBConfig.DatabaseLocalAddress))
            {
                configDBAdapter.EnsureTable<MetadataEntityDBConfig>(MetadataEntityDBConfig.TableName, MetadataEntityDBConfig.TablePrimaryKey);
                configDBAdapter.Open();

                if (!configDBAdapter.Insert(new MetadataEntityDBConfig(tableName, tablePrimaryKey)))
                {
                    LogError($"Can not write config record for entity '{entityClassName}' into database file!");
                    return;
                }
            }

            // Write the entity data into the entity database.
            var ensureTableMethod = EnsureTableMethod.MakeGenericMethod(entityType);
            var insertMethod = InsertMethod.MakeGenericMethod(entityType);

            using (var dataDBAdapter = new IBoxDBAdapter(TempFolderPath, dbLocalAddress))
            {
                ensureTableMethod.Invoke(dataDBAdapter, new object[] { tableName, tablePrimaryKey });
                dataDBAdapter.Open();

                foreach (var data in entityDataList)
                {
                    if (!(bool)insertMethod.Invoke(dataDBAdapter, new[] { tableName, (object)data }))
                        LogWarning($"Can not write metadata ({data}) into database file: {entityClassName}!");
                }
            }

            SaveDatabaseFile(saveFolderPath, $"{entityClassName}{DatabaseFileExtension}", dbLocalAddress, shouldEncryptDatabase, shouldCompressDatabase, cryptoProvider, compressionProvider);
        }

        private static void SaveConfigDatabaseFile(string saveFolderPath,
            bool shouldEncryptDatabase,
            bool shouldCompressDatabase,
            IDatabaseCryptoProvider cryptoProvider,
            IDatabaseCompressionProvider compressionProvider)
        {
            SaveDatabaseFile(saveFolderPath, $"{nameof(MetadataEntityDBConfig)}{DatabaseFileExtension}", MetadataEntityDBConfig.DatabaseLocalAddress, shouldEncryptDatabase, shouldCompressDatabase, cryptoProvider, compressionProvider);
        }

        private static void SaveDatabaseFile(string saveFolderPath,
            string fileName,
            long dbLocalAddress,
            bool shouldEncryptDatabase,
            bool shouldCompressDatabase,
            IDatabaseCryptoProvider cryptoProvider,
            IDatabaseCompressionProvider compressionProvider)
        {
            var sourceFilePath = Path.Combine(TempFolderPath, $"db{dbLocalAddress}.box");
            var rawData = File.ReadAllBytes(sourceFilePath);
            var destFilePath = PathUtility.UnifyToAltDirectorySeparatorChar(Path.Combine(saveFolderPath, fileName));
            File.WriteAllBytes(destFilePath, WrapDatabaseData(rawData, shouldEncryptDatabase, shouldCompressDatabase, cryptoProvider, compressionProvider));
        }

        private static byte[] WrapDatabaseData(byte[] rawData,
            bool shouldEncryptDatabase,
            bool shouldCompressDatabase,
            IDatabaseCryptoProvider cryptoProvider,
            IDatabaseCompressionProvider compressionProvider)
        {
            var encryptionFlag = (byte)(shouldEncryptDatabase ? 1 : 0);
            var compressionFlag = (byte)(shouldCompressDatabase ? 1 : 0);

            using var writer = new BinaryWriter(new MemoryStream());
            writer.Write(encryptionFlag);

            if (shouldEncryptDatabase)
            {
                var key = CryptoUtility.GenerateRandomKey(cryptoProvider.EncryptionKeyLength);
                var output = shouldCompressDatabase ? compressionProvider.Compress(rawData) : rawData;
                writer.Write(key);
                writer.Write(compressionFlag);
                writer.Write(cryptoProvider.Encrypt(output, key));
            }
            else
            {
                var output = shouldCompressDatabase ? compressionProvider.Compress(rawData) : rawData;
                writer.Write(compressionFlag);
                writer.Write(output);
            }

            return ((MemoryStream)writer.BaseStream).ToArray();
        }
        
        private static void LogInfo(string message) => Console.WriteLine($"[INFO] {message}");

        private static void LogError(string message) => Console.WriteLine($"[ERROR] {message}");

        private static void LogWarning(string message) => Console.WriteLine($"[WARNING] {message}");

        private static Type CreateEntityType(ModuleBuilder moduleBuilder, string entityTypeName, IList<EntityPropertyRawInfo> rawInfoList)
        {
            var typeBuilder = moduleBuilder.DefineType(entityTypeName, TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed, typeof(MetadataEntity));

            foreach (var rawInfo in rawInfoList)
            {
                var propertyType = ResolvePropertyType(rawInfo);
                AddAutoImplementedProperty(typeBuilder, rawInfo.PropertyName, propertyType);
            }

            return typeBuilder.CreateType();
        }

        private static void AddAutoImplementedProperty(TypeBuilder typeBuilder, string propertyName, Type propertyType)
        {
            var fieldBuilder = typeBuilder.DefineField($"<{propertyName}>k__BackingField", propertyType, FieldAttributes.Private);
            var propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.None, propertyType, Type.EmptyTypes);

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
                new[] { propertyType });

            ilGenerator = setterMethodBuilder.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Stfld, fieldBuilder);
            ilGenerator.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getterMethodBuilder);
            propertyBuilder.SetSetMethod(setterMethodBuilder);
        }

        private static Type ResolvePropertyType(EntityPropertyRawInfo rawInfo)
        {
            var propertyType = rawInfo.PropertyType;
            var rawInfoEditor = PropertyRawInfoEditorFactory.Instance.GetInstance(propertyType);

            // The Unity type property is stored as the array of its value property type, e.g. float[] for Vector2.
            if (rawInfoEditor is UnityTypeRawInfoEditor)
                return GetPropertyType(rawInfo.Parameters[0] as string);

            // The enum property is stored as the index of its enum value list.
            if (propertyType.Equals(PropertyTypeNames.Enum))
                return typeof(int);

            return GetPropertyType(propertyType);
        }

        private static Type GetPropertyType(string propertyTypeString)
        {
            if (PropertyTypeMap.TryGetValue(propertyTypeString, out var propertyType))
                return propertyType;

            if (propertyTypeString.EndsWith("[]", StringComparison.Ordinal)
                && PropertyTypeMap.TryGetValue(propertyTypeString.Substring(0, propertyTypeString.Length - 2), out var elementType))
                return elementType.MakeArrayType();

            throw new NotSupportedException($"Property type '{propertyTypeString}' is not supported!");
        }

        private static string TempFolderPath
        {
            get
            {
                if (string.IsNullOrEmpty(tempFolderPath))
                {
                    tempFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Temp", "UniSharper", "UniSharper.Data.Metadata", TempFolderName);
                }

                if (!Directory.Exists(tempFolderPath))
                {
                    Directory.CreateDirectory(tempFolderPath);
                }

                return tempFolderPath;
            }
        }

        private static void ResetTempFolder()
        {
            DeleteTempFolder();
            Directory.CreateDirectory(TempFolderPath);
        }

        private static void DeleteTempFolder()
        {
            if (!string.IsNullOrEmpty(tempFolderPath) && Directory.Exists(tempFolderPath))
                Directory.Delete(tempFolderPath, true);
        }
    }
}
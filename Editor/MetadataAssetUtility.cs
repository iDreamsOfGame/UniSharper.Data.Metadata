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
using UnityEditor;
using UnityDebug = UnityEngine.Debug;

// ReSharper disable RedundantArgumentDefaultValue

namespace UniSharperEditor.Data.Metadata
{
    internal static class MetadataAssetUtility
    {
        private const string UnityPackageName = "io.github.idreamsofgame.unisharper.data.metadata";

        private static string tempFolderPath;

        private static string TempFolderPath
        {
            get
            {
                if (string.IsNullOrEmpty(tempFolderPath))
                {
                    tempFolderPath = EditorPath.ConvertToAbsolutePath("Temp", "UniSharper", "UniSharper.Data.Metadata", "Data");
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
            var settings = MetadataAssetSettings.Load();
            MetadataAssetSettings.CreateMetadataPersistentStoreFolder(settings);
            DeleteTempDbFiles();

            FindChangedExcelWorkbookFiles(out var addedExcelFiles, out var updatedExcelFiles, out var deletedExcelFiles);

            var dbFolderPath = EditorPath.ConvertToAbsolutePath(settings.MetadataPersistentStorePath);
            ForEachExcelFile(settings.ExcelWorkbookFilesFolderPath,
                (table, fileName, index, length) =>
                {
                    if (table == null)
                        return;

                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                    var entityClassName = fileNameWithoutExtension.ToTitleCase();
                    var info = $"Creating Database File for Entity {entityClassName}... {index + 1}/{length}";
                    var progress = (float)(index + 1) / length;
                    EditorUtility.DisplayProgressBar("Hold on...", info, progress);

                    var rawInfoList = CreateMetadataEntityRawInfoList(settings, table);
                    var entityClassType = GetEntityClassType(settings, entityClassName);

                    if (entityClassType != null)
                    {
                        var entityDataList = CreateEntityDataList(settings, table, entityClassType, rawInfoList);
                        var updateDatabaseFile = addedExcelFiles.Contains(fileName) || updatedExcelFiles.Contains(fileName);
                        typeof(MetadataAssetUtility).InvokeGenericStaticMethod("InsertEntityData",
                            new[] { entityClassType },
                            new object[] { dbFolderPath, entityClassName, rawInfoList, entityDataList, index, updateDatabaseFile });
                        result = true;
                    }
                    else
                    {
                        UnityDebug.LogErrorFormat(null, "Can not find the entity class: {0}.cs!", entityClassName);
                        result = false;
                    }
                });

            // Copy MetadataEntityDBConfig database file.
            if (result && (addedExcelFiles.Count > 0 || deletedExcelFiles.Count > 0))
            {
                CopyMetadataDatabaseFile(dbFolderPath, MetadataEntityDBConfig.DatabaseLocalAddress);
            }

            // Try delete redundant metadata database file.
            TryDeleteMetadataDatabaseFiles(settings, deletedExcelFiles);

            EditorUtility.ClearProgressBar();
            return result;
        }

        internal static bool GenerateMetadataEntityScripts()
        {
            var result = false;
            var settings = MetadataAssetSettings.Load();
            MetadataAssetSettings.CreateEntityScriptsStoreFolder(settings);

            if (string.IsNullOrEmpty(settings.ExcelWorkbookFilesFolderPath))
            {
                if (EditorUtility.DisplayDialog("Error", "'Excel Workbook Files Folder Path' is not valid path!", "OK"))
                    EditorUtility.ClearProgressBar();
            }
            else
            {
                FindChangedExcelWorkbookFiles(out _, out _, out var deletedExcelFiles);

                // Try delete redundant entity scripts.
                TryDeleteMetadataEntityScripts(settings, deletedExcelFiles);

                ForEachExcelFile(settings.ExcelWorkbookFilesFolderPath,
                    (table, name, index, length) =>
                    {
                        if (table == null)
                            return;

                        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(name);
                        var info = $"Generating Metadata Entity Script: {fileNameWithoutExtension}.cs... {index + 1}/{length}";
                        var progress = (float)(index + 1) / length;
                        EditorUtility.DisplayProgressBar("Hold on...", info, progress);
                        var rawInfoList = CreateMetadataEntityRawInfoList(settings, table);
                        result = GenerateMetadataEntityScript(settings, fileNameWithoutExtension, rawInfoList);
                    });
            }

            EditorUtility.ClearProgressBar();
            return result;
        }

        internal static void SaveExcelWorkbookFileHashMap()
        {
            var settings = MetadataAssetSettings.Load();
            var paths = FileUtility.GetExcelFilePathCollection(settings.ExcelWorkbookFilesFolderPath);
            if (paths.Length == 0)
                return;

            var fileHashMap = new Dictionary<string, string>();
            foreach (var path in paths)
            {
                var fileName = Path.GetFileName(path);
                var rawData = File.ReadAllBytes(path);
                var hashString = CryptoUtility.Md5HashEncrypt(Encoding.UTF8.GetString(rawData));
                fileHashMap.Add(fileName, hashString);
            }

            ExcelWorkbookFileHashMap.Save(fileHashMap);
        }

        internal static void ClearExcelWorkbookFileHashMap()
        {
            ExcelWorkbookFileHashMap.Delete();
        }

        private static void FindChangedExcelWorkbookFiles(out List<string> addedExcelFiles, out List<string> updatedExcelFiles, out List<string> deletedExcelFiles)
        {
            addedExcelFiles = new List<string>();
            updatedExcelFiles = new List<string>();
            deletedExcelFiles = new List<string>();

            var oldHashMap = ExcelWorkbookFileHashMap.Load();
            var settings = MetadataAssetSettings.Load();
            var paths = FileUtility.GetExcelFilePathCollection(settings.ExcelWorkbookFilesFolderPath);
            if (paths.Length == 0)
            {
                if (oldHashMap.Count > 0)
                    deletedExcelFiles = oldHashMap.Keys.ToList();
                return;
            }

            var newHashMap = new Dictionary<string, string>();
            foreach (var path in paths)
            {
                var fileName = Path.GetFileName(path);
                var rawData = File.ReadAllBytes(path);
                var hashString = CryptoUtility.Md5HashEncrypt(Encoding.UTF8.GetString(rawData));
                newHashMap.Add(fileName, hashString);

                if (oldHashMap.ContainsKey(fileName))
                {
                    // Updated file.
                    if (!string.IsNullOrEmpty(oldHashMap[fileName]) && oldHashMap[fileName] != hashString)
                        updatedExcelFiles.Add(fileName);
                }
                else
                {
                    // New file.
                    addedExcelFiles.Add(fileName);
                }
            }

            // Find deleted files.
            deletedExcelFiles.AddRange(oldHashMap.Keys.Where(fileName => !newHashMap.ContainsKey(fileName)));
        }

        private static void CopyMetadataDatabaseFile(string dbFolderPath, long dbLocalAddress, string entityName = null)
        {
            var sourceFilePath = PathUtility.UnifyToAltDirectorySeparatorChar(Path.Combine(TempFolderPath, $"db{dbLocalAddress}.box"));
            var newFileName = dbLocalAddress > 1 ? $"{entityName}.db.bytes" : "MetadataEntityDBConfig.db.bytes";
            var destFilePath = PathUtility.UnifyToAltDirectorySeparatorChar(Path.Combine(dbFolderPath, newFileName));
            FileUtil.ReplaceFile(sourceFilePath, destFilePath);
            EncryptFileRawData(destFilePath);
            var assetFilePath = EditorPath.ConvertToAssetPath(destFilePath);
            AssetDatabase.ImportAsset(assetFilePath);
        }

        private static List<MetadataEntity> CreateEntityDataList(MetadataAssetSettings settings,
            DataTable table,
            Type entityClassType,
            List<EntityPropertyRawInfo> rawInfoList)
        {
            var list = new List<MetadataEntity>();
            var rowCount = table.Rows.Count;

            for (var i = settings.EntityDataStartingRowIndex; i < rowCount; ++i)
            {
                var entityData = (MetadataEntity)entityClassType.InvokeConstructor();

                for (int j = 0, propertiesCount = rawInfoList.Count; j < propertiesCount; ++j)
                {
                    var cellValue = table.Rows[i][j].ToString();
                    var rowInfo = rawInfoList[j];
                    var typeParser = PropertyTypeConverterFactory.Instance.GetInstance(rowInfo.PropertyType);

                    if (typeParser != null)
                    {
                        var arrayElementSeparator = settings.ArrayElementSeparator;
                        var value = typeParser.Parse(arrayElementSeparator, cellValue.Trim(), rowInfo.Parameters);
                        entityData.SetPropertyValue(rowInfo.PropertyName, value);
                    }
                    else
                    {
                        UnityDebug.LogWarningFormat("Type '{0}' is not supported!", rowInfo.PropertyType);
                    }
                }

                list.Add(entityData);
            }

            return list;
        }

        private static List<EntityPropertyRawInfo> CreateMetadataEntityRawInfoList(MetadataAssetSettings settings, DataTable table)
        {
            var columns = table.Columns;
            var rows = table.Rows;
            var columnCount = columns.Count;
            var list = new List<EntityPropertyRawInfo>();

            for (var column = 0; column < columnCount; column++)
            {
                var propertyName = rows[settings.EntityPropertyNameRowIndex][column].ToString();
                var propertyType = rows[settings.EntityPropertyTypeRowIndex][column].ToString();
                var comment = rows[settings.EntityPropertyCommentRowIndex][column].ToString();

                if (string.IsNullOrEmpty(propertyName) || string.IsNullOrEmpty(propertyType))
                    continue;

                propertyName = propertyName.Trim().ToTitleCase();
                propertyType = propertyType.Trim();
                comment = FormatCommentString(comment.Trim());

                var editor = PropertyRawInfoEditorFactory.Instance.GetInstance(propertyType);
                var rawInfo = editor?.Edit(settings, table, column, comment, propertyType, propertyName)
                              ?? new EntityPropertyRawInfo(comment, propertyType, propertyName);
                list.Add(rawInfo);
            }

            return list;
        }

        private static void DeleteTempDbFiles()
        {
            FileUtil.DeleteFileOrDirectory(TempFolderPath);
        }

        private static void EncryptFileRawData(string filePath)
        {
            var settings = MetadataAssetSettings.Load();
            var rawData = File.ReadAllBytes(filePath);
            var dataEncryptionFlag = BitConverter.GetBytes(settings.DataEncryptionAndDecryption);
            using var writer = new BinaryWriter(new MemoryStream());
            // Write flag to judge if data need to decryption
            writer.Write(dataEncryptionFlag);

            if (settings.DataEncryptionAndDecryption)
            {
                // Write key and cipher data
                var key = CryptoUtility.GenerateRandomKey(MetadataManager.EncryptionKeyLength);
                var cipherData = CryptoUtility.AesEncrypt(rawData, key);
                writer.Write(key);
                writer.Write(cipherData);
            }
            else
            {
                // Write raw data
                writer.Write(rawData);
            }

            var bufferData = ((MemoryStream)writer.BaseStream).GetBuffer();
            File.WriteAllBytes(filePath, bufferData);
        }

        private static void ForEachExcelFile(string excelFilesFolderPath, Action<DataTable, string, int, int> action)
        {
            if (string.IsNullOrEmpty(excelFilesFolderPath))
            {
                UnityDebug.LogWarning("Excel files folder path can not be null or empty!");
                return;
            }

            // Find *.xls and *.xlsx
            var paths = FileUtility.GetExcelFilePathCollection(excelFilesFolderPath);
            if (paths.Length == 0)
            {
                UnityDebug.LogError("No excel files found!");
                action?.Invoke(null, null, -1, -1);
                return;
            }

            for (var i = 0; i < paths.Length; i++)
            {
                var path = paths[i];
                var fileName = Path.GetFileName(path);

                using var stream = File.OpenRead(path);

                try
                {
                    using var reader = ExcelReaderFactory.CreateReader(stream);
                    if (reader != null)
                    {
                        var result = reader.AsDataSet();
                        if (result is { Tables: { Count: > 0 } })
                        {
                            var table = result.Tables[0];
                            if (table != null)
                                action?.Invoke(table, fileName, i, paths.Length);
                        }
                        else
                        {
                            UnityDebug.LogError("Excel file should have a table at least!");
                        }
                    }
                }
                catch (Exception e)
                {
                    UnityDebug.LogError($"fileName: {fileName}, error: {e}");
                    EditorUtility.ClearProgressBar();
                    throw;
                }
            }
        }

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
                if (rawInfo.Parameters[2] is not string[] enumValueList)
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

                var scriptFilePath = EditorPath.ConvertToAbsolutePath(settings.EntityScriptsStorePath, $"{entityScriptName}.cs");
                var scriptAssetPath = EditorPath.ConvertToAssetPath(scriptFilePath);
                File.WriteAllText(scriptFilePath, scriptTextContent, new UTF8Encoding(true));
                AssetDatabase.ImportAsset(scriptAssetPath);
                return true;
            }
            catch (Exception exception)
            {
                UnityDebug.LogError(exception.ToString());
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
        private static void InsertEntityData<T>(string dbFolderPath,
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
            var success = dbAdapter.Insert(new MetadataEntityDBConfig(tableName, tablePrimaryKey));

            if (!success)
                return;

            var dataList = new T[entityDataList.Count];

            for (var i = 0; i < entityDataList.Count; i++)
            {
                dataList[i] = (T)entityDataList[i];
            }

            using var dataDBAdapter = new IBoxDBAdapter(TempFolderPath, dbLocalAddress);
            dataDBAdapter.EnsureTable<T>(typeof(T).Name, tablePrimaryKey);
            dataDBAdapter.Open();

            foreach (var data in dataList)
            {
                var result = dataDBAdapter.Insert(tableName, data);

                if (!result)
                {
                    UnityDebug.LogWarning($"Can not write metadata ({data}) into database file: {tableName}!");
                }
            }

            // Copy metadata entity database file.
            if (updateDatabaseFile)
                CopyMetadataDatabaseFile(dbFolderPath, dbLocalAddress, tableName);
        }

        private static void TryDeleteMetadataEntityScripts(MetadataAssetSettings settings, List<string> deletedExcelFiles)
        {
            if (settings.DeleteRedundantMetadataAndEntityScripts && deletedExcelFiles.Count > 0)
            {
                foreach (var deletedExcelFile in deletedExcelFiles)
                {
                    DeleteMetadataEntityScript(settings, deletedExcelFile);
                }

                AssetDatabase.Refresh();
            }
        }

        private static void DeleteMetadataEntityScript(MetadataAssetSettings settings, string excelFileName)
        {
            var entityScriptName = Path.GetFileNameWithoutExtension(excelFileName);
            var scriptFilePath = Path.Combine(settings.EntityScriptsStorePath, $"{entityScriptName}.cs");
            if (AssetDatabase.DeleteAsset(scriptFilePath))
                return;

            // Plan B: Try delete file by using class FileUtil.
            var scriptMetaFilePath = $"{scriptFilePath}.meta";
            FileUtil.DeleteFileOrDirectory(scriptFilePath);
            FileUtil.DeleteFileOrDirectory(scriptMetaFilePath);
        }

        private static void TryDeleteMetadataDatabaseFiles(MetadataAssetSettings settings, List<string> deletedExcelFiles)
        {
            if (settings.DeleteRedundantMetadataAndEntityScripts && deletedExcelFiles.Count > 0)
            {
                foreach (var deletedExcelFile in deletedExcelFiles)
                {
                    DeleteMetadataDatabaseFile(settings, deletedExcelFile);
                }

                AssetDatabase.Refresh();
            }
        }

        private static void DeleteMetadataDatabaseFile(MetadataAssetSettings settings, string excelFileName)
        {
            var metadataName = Path.GetFileNameWithoutExtension(excelFileName);
            var databaseFilePath = Path.Combine(settings.MetadataPersistentStorePath, $"{metadataName}.db");
            if (AssetDatabase.DeleteAsset(databaseFilePath))
                return;

            // Plan B: Try delete file by using class FileUtil.
            databaseFilePath = $"{databaseFilePath}.bytes";
            var scriptMetaFilePath = $"{databaseFilePath}.meta";
            FileUtil.DeleteFileOrDirectory(databaseFilePath);
            FileUtil.DeleteFileOrDirectory(scriptMetaFilePath);
        }
    }
}
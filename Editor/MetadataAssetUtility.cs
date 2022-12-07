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
using ReSharp.Security.Cryptography;
using UniSharper;
using UniSharper.Data.Metadata;
using UniSharperEditor.Data.Metadata.Converters;
using UnityEditor;
using UnityEngine;
using UnityDebug = UnityEngine.Debug;
using UnityEditorUtility = UnityEditor.EditorUtility;
// ReSharper disable RedundantArgumentDefaultValue

namespace UniSharperEditor.Data.Metadata
{
    internal static class MetadataAssetUtility
    {
        private const string UnityPackageName = "io.github.idreamsofgame.unisharper.data.metadata";

        private const string ExcelFileExtension = ".xlsx";

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
            
            GetChangedExcelWorkbookFiles(out var addedExcelFiles, out var updatedExcelFiles, out var deletedExcelFiles);

            var dbFolderPath = EditorPath.ConvertToAbsolutePath(settings.MetadataPersistentStorePath);
            ForEachExcelFile(settings.ExcelWorkbookFilesFolderPath, (table, fileName, fileNameWithoutExtension, index, length) =>
            {
                if (table == null)
                    return;
                
                var entityClassName = fileNameWithoutExtension.ToTitleCase();
                var info = $"Creating Database File for Entity {entityClassName}... {index + 1}/{length}";
                var progress = (float)(index + 1) / length;
                UnityEditorUtility.DisplayProgressBar("Hold on...", info, progress);

                var rawInfoList = CreateMetadataEntityRawInfoList(settings, table);
                var entityClassType = GetEntityClassType(settings, entityClassName);

                if (entityClassType != null)
                {
                    var entityDataList = CreateEntityDataList(settings, table, entityClassType, rawInfoList);
                    typeof(MetadataAssetUtility).InvokeGenericStaticMethod("InsertEntityData", 
                        new[] { entityClassType }, 
                        new object[] { dbFolderPath, entityClassName, rawInfoList, entityDataList, index });
                    result = true;
                }
                else
                {
                    UnityDebug.LogErrorFormat(null, "Can not find the entity class: {0}.cs!", entityClassName);
                    result = false;
                }
            });

            // Copy MetadataEntityDBConfig database file.
            if (result)
            {
                CopyDatabaseFile(dbFolderPath, MetadataEntityDBConfig.DatabaseLocalAddress);
            }

            UnityEditorUtility.ClearProgressBar();
            return result;
        }

        internal static bool GenerateMetadataEntityScripts()
        {
            var result = false;
            var settings = MetadataAssetSettings.Load();
            MetadataAssetSettings.CreateEntityScriptsStoreFolder(settings);

            if (string.IsNullOrEmpty(settings.ExcelWorkbookFilesFolderPath))
            {
                if (UnityEditorUtility.DisplayDialog("Error", "'Excel Workbook Files Folder Path' is not valid path!", "OK"))
                    UnityEditorUtility.ClearProgressBar();
            }
            else
            {
                GetChangedExcelWorkbookFiles(out var addedExcelFiles, out var updatedExcelFiles, out var deletedExcelFiles);
                
                ForEachExcelFile(settings.ExcelWorkbookFilesFolderPath, (table, fileName, fileNameWithoutExtension, index, length) =>
                {
                    if (table == null) 
                        return;

                    // Remove redundant entity script.
                    if (settings.DeleteRedundantMetadataAndEntityScripts && deletedExcelFiles.Count > 0 && deletedExcelFiles.Contains(fileName))
                    {
                        DeleteMetadataEntityScript(settings, fileName);
                        return;
                    }

                    var info = $"Generating Metadata Entity Script: {fileNameWithoutExtension}.cs... {index + 1}/{length}";
                    var progress = (float)(index + 1) / length;
                    UnityEditorUtility.DisplayProgressBar("Hold on...", info, progress);
                    var rawInfoList = CreateMetadataEntityRawInfoList(settings, table);
                    result = GenerateMetadataEntityScript(settings, fileNameWithoutExtension, rawInfoList);
                });
            }

            UnityEditorUtility.ClearProgressBar();
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
        
        private static void GetChangedExcelWorkbookFiles(out List<string> addedExcelFiles, out List<string> updatedExcelFiles, out List<string> deletedExcelFiles)
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

        private static void CopyDatabaseFile(string dbFolderPath, long dbLocalAddress, string entityName = null)
        {
            var sourceFilePath = PathUtility.UnifyToAltDirectorySeparatorChar(Path.Combine(TempFolderPath, $"db{dbLocalAddress}.box"));
            var newFileName = dbLocalAddress > 1 ? $"{entityName}.db.bytes" : "MetadataEntityDBConfig.db.bytes";
            var destFilePath = PathUtility.UnifyToAltDirectorySeparatorChar(Path.Combine(dbFolderPath, newFileName));
            FileUtil.ReplaceFile(sourceFilePath, destFilePath);
            EncryptFileRawData(destFilePath);
            var assetFilePath = EditorPath.ConvertToAssetPath(destFilePath);
            AssetDatabase.ImportAsset(assetFilePath);
        }

        private static List<MetadataEntity> CreateEntityDataList(MetadataAssetSettings settings, DataTable table, Type entityClassType, List<MetadataEntityRawInfo> rawInfoList)
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
                    var typeParser = PropertyTypeConverterFactory.GetTypeConverter(rowInfo.PropertyType, rowInfo.PropertyName);

                    if (typeParser != null)
                    {
                        var value = typeParser.Parse(cellValue.Trim(), rowInfo.Parameters);
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

        private static List<MetadataEntityRawInfo> CreateMetadataEntityRawInfoList(MetadataAssetSettings settings, DataTable table)
        {
            var columns = table.Columns;
            var rows = table.Rows;
            var columnCount = columns.Count;
            var list = new List<MetadataEntityRawInfo>();

            for (var i = 0; i < columnCount; i++)
            {
                var propertyName = rows[settings.EntityPropertyNameRowIndex][i].ToString();
                var propertyType = rows[settings.EntityPropertyTypeRowIndex][i].ToString();
                var comment = rows[settings.EntityPropertyCommentRowIndex][i].ToString();

                if (string.IsNullOrEmpty(propertyName) || string.IsNullOrEmpty(propertyType))
                    continue;

                propertyName = propertyName.Trim().ToTitleCase();
                propertyType = propertyType.Trim().ToLower();
                comment = FormatCommentString(comment.Trim());

                object[] arguments = null;

                if (propertyType.Equals("enum"))
                {
                    arguments = new object[3];
                    arguments[0] = propertyName;
                    arguments[1] = $"{propertyName}Enum";

                    var enumValues = new List<string> { "None" };
                    for (var j = settings.EntityDataStartingRowIndex; j < rows.Count; j++)
                    {
                        var enumValue = rows[j][i].ToString().Trim();

                        if (!string.IsNullOrEmpty(enumValue))
                        {
                            enumValues.AddUnique(enumValue);
                        }
                    }

                    arguments[2] = enumValues.ToArray();
                    propertyName = $"{propertyName}IntValue";
                }
                
                list.Add(new MetadataEntityRawInfo(comment, propertyType, propertyName, arguments));
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

        private static void ForEachExcelFile(string excelFilesFolderPath, Action<DataTable, string, string, int, int> action)
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

                action?.Invoke(null, null, null, -1, -1);
                return;
            }

            for (var i = 0; i < paths.Length; i++)
            {
                var path = paths[i];
                var fileName = Path.GetFileName(path);
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
                var fileExtension = Path.GetExtension(path);

                using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                using var reader = fileExtension is ExcelFileExtension 
                    ? ExcelReaderFactory.CreateOpenXmlReader(stream) 
                    : ExcelReaderFactory.CreateBinaryReader(stream);
                var result = reader.AsDataSet();

                if (result.Tables.Count > 0)
                {
                    var table = result.Tables[0];
                    action?.Invoke(table, fileName, fileNameWithoutExtension, i, paths.Length);
                }
                else
                {
                    UnityDebug.LogError("Excel file should have a table at least!");
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

        private static string GenerateEntityScriptEnumString(List<MetadataEntityRawInfo> rawInfoList)
        {
            var stringBuilder = new StringBuilder();

            for (int i = 0, length = rawInfoList.Count; i < length; ++i)
            {
                var rowInfo = rawInfoList[i];

                if (!rowInfo.PropertyType.Equals("enum"))
                    continue;

                var enumValuesString = new StringBuilder();
                if (rowInfo.Parameters[2] is not string[] enumValueList)
                    continue;

                for (var j = 0; j < enumValueList.Length; j++)
                {
                    var enumValue = enumValueList[j];
                    enumValuesString.Append($"\t\t\t{enumValue}");

                    if (j >= enumValueList.Length - 1)
                        continue;

                    enumValuesString.Append(",").Append(PlayerEnvironment.WindowsNewLine);
                }

                stringBuilder.AppendFormat(ScriptTemplate.ClassMemberFormatString.EnumDefinition, rowInfo.Comment, rowInfo.Parameters[1], enumValuesString)
                    .Append(PlayerEnvironment.WindowsNewLine).Append(PlayerEnvironment.WindowsNewLine);
            }

            return stringBuilder.ToString();
        }

        private static string GenerateEntityScriptPropertiesString(List<MetadataEntityRawInfo> rawInfoList)
        {
            var stringBuilder = new StringBuilder();

            for (int i = 0, length = rawInfoList.Count; i < length; ++i)
            {
                var rowInfo = rawInfoList[i];
                var propertyType = rowInfo.PropertyType;

                if (propertyType.Equals("enum"))
                {
                    // Add enum int value property
                    stringBuilder.AppendFormat(ScriptTemplate.ClassMemberFormatString.PropertyMember, rowInfo.Comment, "int", rowInfo.PropertyName);

                    stringBuilder.Append(PlayerEnvironment.WindowsNewLine)
                        .Append(PlayerEnvironment.WindowsNewLine);

                    // Add enum property
                    stringBuilder.AppendFormat(ScriptTemplate.ClassMemberFormatString.EnumProperty, rowInfo.Comment, rowInfo.Parameters[1], rowInfo.Parameters[0], rowInfo.PropertyName);
                }
                else
                {
                    stringBuilder.AppendFormat(ScriptTemplate.ClassMemberFormatString.PropertyMember, rowInfo.Comment, propertyType, rowInfo.PropertyName);
                }

                if (i >= length - 1)
                    continue;

                stringBuilder.Append(PlayerEnvironment.WindowsNewLine)
                    .Append(PlayerEnvironment.WindowsNewLine);
            }

            return stringBuilder.ToString();
        }

        private static bool GenerateMetadataEntityScript(MetadataAssetSettings settings, string entityScriptName, List<MetadataEntityRawInfo> rawInfoList)
        {
            try
            {
                entityScriptName = entityScriptName.ToTitleCase();
                var scriptTextContent = ScriptTemplate.LoadScriptTemplateFile("NewMetadataEntityScriptTemplate.txt", UnityPackageName);
                scriptTextContent = scriptTextContent.Replace(ScriptTemplate.Placeholders.Namespace, settings.EntityScriptNamespace);
                scriptTextContent = scriptTextContent.Replace(ScriptTemplate.Placeholders.ScriptName, entityScriptName);
                scriptTextContent = scriptTextContent.Replace(ScriptTemplate.Placeholders.EnumInsideOfClass, GenerateEntityScriptEnumString(rawInfoList));
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

        private static void DeleteMetadataEntityScript(MetadataAssetSettings settings, string entityScriptName)
        {
            var scriptFilePath = Path.Combine(settings.EntityScriptsStorePath, $"{entityScriptName}.cs");
            if (AssetDatabase.DeleteAsset(scriptFilePath)) 
                return;
            
            // Plan B: Try delete file by using class FileUtil.
            var scriptMetaFilePath = $"{scriptFilePath}.meta";
            FileUtil.DeleteFileOrDirectory(scriptFilePath);
            FileUtil.DeleteFileOrDirectory(scriptMetaFilePath);
        }

        private static Type GetEntityClassType(MetadataAssetSettings settings, string entityClassName)
        {
            var scriptAssetPath = PathUtility.UnifyToAltDirectorySeparatorChar(Path.Combine(settings.EntityScriptsStorePath, $"{entityClassName}.cs"));
            var scriptObject = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptAssetPath);
            return scriptObject ? scriptObject.GetClass() : null;
        }

        [UsedImplicitly]
        private static void InsertEntityData<T>(string dbFolderPath, string tableName, IList<MetadataEntityRawInfo> rowInfos, IList<MetadataEntity> entityDataList, int index) where T : MetadataEntity
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
                    Debug.LogWarning($"Can not write metadata ({data}) into database file: {tableName}!");
                }
            }

            // Copy metadata entity database file.
            CopyDatabaseFile(dbFolderPath, dbLocalAddress, tableName);
        }

        private readonly struct MetadataEntityRawInfo
        {
            public MetadataEntityRawInfo(string comment, string propertyType, string propertyName, params object[] parameters)
            {
                Comment = comment;
                PropertyType = propertyType;
                PropertyName = propertyName;
                Parameters = parameters;
            }

            public string Comment { get; }

            public object[] Parameters { get; }

            public string PropertyName { get; }

            public string PropertyType { get; }
        }
    }
}

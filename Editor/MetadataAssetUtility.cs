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
using ReSharp.Data.iBoxDB;
using UniSharper;
using UniSharper.Data.Metadata;
using UniSharper.Data.Metadata.Parsers;
using UniSharperEditor.Data.Metadata.Converters;
using UnityEditor;
using UnityEngine;
using UnityDebug = UnityEngine.Debug;
using UnityEditorUtility = UnityEditor.EditorUtility;

namespace UniSharperEditor.Data.Metadata
{
    internal static class MetadataAssetUtility
    {
        #region Fields

        private const string UnityPackageName = "io.github.idreamsofgame.unisharper.data.metadata";

        private static string tempFolderPath;

        #endregion Fields

        #region Properties

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

        #endregion Properties

        #region Methods

        internal static bool CreateMetadataDatabaseFiles()
        {
            var result = false;
            var settings = MetadataAssetSettings.Load();
            MetadataAssetSettings.CreateMetadataPersistentStoreFolder(settings);
            DeleteTempDbFiles();

            string dbFolderPath = EditorPath.ConvertToAbsolutePath(settings.MetadataPersistentStorePath);

            ForEachExcelFile(settings.ExcelWorkbookFilesFolderPath, (table, fileName, index, length) =>
            {
                result = (table != null);
                if (!result) 
                    return;
                var entityClassName = fileName.ToTitleCase();
                var info = $"Creating Database File for Entity {entityClassName}... {index + 1}/{length}";
                var progress = (float)(index + 1) / length;
                UnityEditorUtility.DisplayProgressBar("Hold on...", info, progress);

                var rawInfoList = CreateMetadataEntityRawInfoList(settings, entityClassName, table);
                var entityClassType = GetEntityClassType(settings, entityClassName);

                if (entityClassType != null)
                {
                    var entityDataList = CreateEntityDataList(settings, table, entityClassType, rawInfoList);
                    typeof(MetadataAssetUtility).InvokeGenericStaticMethod("InsertEntityData", new[] { entityClassType }, new object[] { dbFolderPath, entityClassName, rawInfoList, entityDataList, index });
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
                UnityEditorUtility.DisplayDialog("Error", "'Excel Workbook Files Folder Path' is not valid path!", "OK");
            }
            else
            {
                ForEachExcelFile(settings.ExcelWorkbookFilesFolderPath, (table, fileName, index, length) =>
                {
                    result = (table != null);
                    if (result)
                    {
                        var info = $"Generating Metadata Entity Script: {fileName}.cs... {index + 1}/{length}";
                        var progress = (float)(index + 1) / length;
                        UnityEditorUtility.DisplayProgressBar("Hold on...", info, progress);
                        var rawInfoList = CreateMetadataEntityRawInfoList(settings, fileName, table);
                        result = GenerateMetadataEntityScript(settings, fileName, rawInfoList);
                    }
                });
            }

            UnityEditorUtility.ClearProgressBar();
            return result;
        }

        private static void CopyDatabaseFile(string dbFolderPath, long dbLocalAddress, string entityName = null)
        {
            var sourceFilePath = PathUtility.UnifyToAltDirectorySeparatorChar(Path.Combine(TempFolderPath, $"db{dbLocalAddress}.box"));
            var newFileName = dbLocalAddress > 1 ? $"{entityName}.db.bytes" : "MetadataEntityDBConfig.db.bytes";
            var destFilePath = PathUtility.UnifyToAltDirectorySeparatorChar(Path.Combine(dbFolderPath, newFileName));
            FileUtil.ReplaceFile(sourceFilePath, destFilePath);
            var assetFilePath = EditorPath.ConvertToAssetPath(destFilePath);
            AssetDatabase.ImportAsset(assetFilePath);
        }

        private static List<MetadataEntity> CreateEntityDataList(MetadataAssetSettings settings, DataTable table, Type entityClassType, List<MetadataEntityRawInfo> rawInfoList)
        {
            List<MetadataEntity> list = new List<MetadataEntity>();

            int rowCount = table.Rows.Count;

            for (int i = settings.EntityDataStartingRowIndex; i < rowCount; ++i)
            {
                MetadataEntity entityData = (MetadataEntity)entityClassType.InvokeConstructor();

                for (int j = 0, propertiesCount = rawInfoList.Count; j < propertiesCount; ++j)
                {
                    string cellValue = table.Rows[i][j].ToString().Trim();
                    MetadataEntityRawInfo rowInfo = rawInfoList[j];
                    IPropertyTypeConverter typeParser = PropertyTypeConverterFactory.GetTypeConverter(rowInfo.PropertyType, rowInfo.PropertyName);

                    if (typeParser != null)
                    {
                        object value = typeParser.Parse(cellValue, rowInfo.Parameters);
                        entityData.SetObjectPropertyValue(rowInfo.PropertyName, value);
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

        private static List<MetadataEntityRawInfo> CreateMetadataEntityRawInfoList(MetadataAssetSettings settings, string entityName, DataTable table)
        {
            DataColumnCollection columns = table.Columns;
            DataRowCollection rows = table.Rows;
            int columnCount = columns.Count;
            List<MetadataEntityRawInfo> list = new List<MetadataEntityRawInfo>();

            for (int i = 0; i < columnCount; i++)
            {
                string propertyName = rows[settings.EntityPropertyNameRowIndex][i].ToString().Trim().ToTitleCase();
                string propertyType = rows[settings.EntityPropertyTypeRowIndex][i].ToString().Trim().ToLower();
                string comment = FormatCommentString(rows[settings.EntityPropertyCommentRowIndex][i].ToString().Trim());

                if (!string.IsNullOrEmpty(propertyName) && !string.IsNullOrEmpty(propertyType))
                {
                    object[] arguments = null;

                    if (propertyType.Equals("enum"))
                    {
                        arguments = new object[3];
                        arguments[0] = propertyName;
                        arguments[1] = $"{propertyName}Enum";

                        List<string> enumValues = new List<string>
                        {
                            "None"
                        };

                        for (int j = settings.EntityDataStartingRowIndex; j < rows.Count; j++)
                        {
                            string enumValue = rows[j][i].ToString().Trim();

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
            }

            return list;
        }

        private static void DeleteTempDbFiles()
        {
            FileUtil.DeleteFileOrDirectory(TempFolderPath);
        }

        private static void ForEachExcelFile(string excelFilesFolderPath, Action<DataTable, string, int, int> action)
        {
            if (string.IsNullOrEmpty(excelFilesFolderPath))
            {
                UnityDebug.LogWarning("Excel files folder path can not be null or empty!");
                return;
            }

            string[] paths = Directory.GetFiles(excelFilesFolderPath, "*.xls", SearchOption.AllDirectories);

            // Mac OS can not find *.xlsx files by above filter.
            if (!PlayerEnvironment.IsWindowsEditorPlatform)
            {
                string[] xlsxFilePaths = Directory.GetFiles(excelFilesFolderPath, "*.xlsx", SearchOption.AllDirectories);
                paths = paths.Concat(xlsxFilePaths).ToArray();
            }

            if (paths.Length == 0)
            {
                UnityDebug.LogError("No excel files found!");

                action?.Invoke(null, null, -1, -1);
                return;
            }

            for (int i = 0; i < paths.Length; i++)
            {
                string path = paths[i];
                string fileName = Path.GetFileNameWithoutExtension(path);
                string fileExtension = Path.GetExtension(path);

                using (FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read))
                {
                    using (IExcelDataReader reader = fileExtension != null && fileExtension.Equals(".xlsx") ? ExcelReaderFactory.CreateOpenXmlReader(stream) : ExcelReaderFactory.CreateBinaryReader(stream))
                    {
                        DataSet result = reader.AsDataSet();

                        if (result.Tables.Count > 0)
                        {
                            DataTable table = result.Tables[0];

                            action?.Invoke(table, fileName, i, paths.Length);
                        }
                        else
                        {
                            UnityDebug.LogError("Excel file should have a table at least!");
                            continue;
                        }
                    }
                }
            }
        }

        private static string FormatCommentString(string comment)
        {
            if (!string.IsNullOrEmpty(comment))
            {
                const string pattern = @"\r*\n";
                Regex rgx = new Regex(pattern);
                return rgx.Replace(comment, PlayerEnvironment.WindowsNewLine + "\t\t/// ");
            }

            return comment;
        }

        private static string GenerateEntityScriptEnumString(List<MetadataEntityRawInfo> rawInfoList)
        {
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0, length = rawInfoList.Count; i < length; ++i)
            {
                MetadataEntityRawInfo rowInfo = rawInfoList[i];

                if (rowInfo.PropertyType.Equals("enum"))
                {
                    StringBuilder enumValuesString = new StringBuilder();
                    string[] enumValueList = rowInfo.Parameters[2] as string[];

                    for (int j = 0; j < enumValueList.Length; j++)
                    {
                        var enumValue = enumValueList[j];
                        enumValuesString.Append($"\t\t\t{enumValue}");

                        if (j >= enumValueList.Length - 1)
                            continue;

                        enumValuesString.Append(",")
                            .AppendWindowsNewLine();
                    }

                    stringBuilder.AppendFormat(ScriptTemplate.ClassMemeberFormatString.EnumDefinition, rowInfo.Comment, rowInfo.Parameters[1], enumValuesString)
                        .AppendWindowsNewLine().AppendWindowsNewLine();
                }
            }

            return stringBuilder.ToString();
        }

        private static string GenerateEntityScriptPropertiesString(List<MetadataEntityRawInfo> rawInfoList)
        {
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0, length = rawInfoList.Count; i < length; ++i)
            {
                MetadataEntityRawInfo rowInfo = rawInfoList[i];
                string propertyType = rowInfo.PropertyType;

                if (propertyType.Equals("enum"))
                {
                    // Add enum int value property
                    stringBuilder.AppendFormat(ScriptTemplate.ClassMemeberFormatString.PropertyMember, rowInfo.Comment, "int", rowInfo.PropertyName);

                    stringBuilder.AppendWindowsNewLine()
                        .AppendWindowsNewLine();
                    
                    // Add enum property
                    stringBuilder.AppendFormat(ScriptTemplate.ClassMemeberFormatString.EnumProperty, rowInfo.Comment, rowInfo.Parameters[1], rowInfo.Parameters[0], rowInfo.PropertyName);
                }
                else
                {
                    stringBuilder.AppendFormat(ScriptTemplate.ClassMemeberFormatString.PropertyMember, rowInfo.Comment, propertyType, rowInfo.PropertyName);
                }

                if (i >= length - 1)
                    continue;

                stringBuilder.AppendWindowsNewLine()
                    .AppendWindowsNewLine();
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

                var scriptStorePath = EditorPath.ConvertToAbsolutePath(settings.EntityScriptsStorePath, $"{entityScriptName}.cs");
                var assetPath = EditorPath.ConvertToAssetPath(scriptStorePath);
                File.WriteAllText(scriptStorePath, scriptTextContent, new UTF8Encoding(true));
                AssetDatabase.ImportAsset(assetPath);
                return true;
            }
            catch (Exception ex)
            {
                UnityDebug.LogError(ex.ToString());
                return false;
            }
        }

        private static Type GetEntityClassType(MetadataAssetSettings settings, string entityClassName)
        {
            var scriptAssetPath = PathUtility.UnifyToAltDirectorySeparatorChar(Path.Combine(settings.EntityScriptsStorePath, $"{entityClassName}.cs"));
            var scriptObject = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptAssetPath);
            return scriptObject ? scriptObject.GetClass() : null;
        }

        private static void InsertEntityData<T>(string dbFolderPath, string tableName, IList<MetadataEntityRawInfo> rowInfos, IList<MetadataEntity> entityDataList, int index) where T : MetadataEntity
        {
            using (var configDbContext = new iBoxDBContext(TempFolderPath, MetadataEntityDBConfig.DatabaseLocalAddress))
            {
                configDbContext.EnsureTable<MetadataEntityDBConfig>(typeof(MetadataEntityDBConfig).Name, MetadataEntityDBConfig.TablePrimaryKey);
                configDbContext.Open();
                var tablePrimaryKey = rowInfos[0].PropertyName;
                long dbLocalAddress = index + 2;
                var success = configDbContext.Insert(new MetadataEntityDBConfig(tableName, tablePrimaryKey));

                if (!success) return;
                var dataList = new T[entityDataList.Count];

                for (var i = 0; i < entityDataList.Count; i++)
                {
                    dataList[i] = (T)entityDataList[i];
                }

                bool result;
                using (var dbContext = new iBoxDBContext(TempFolderPath, dbLocalAddress))
                {
                    dbContext.EnsureTable<T>(typeof(T).Name, tablePrimaryKey);
                    dbContext.Open();

                    foreach (var data in dataList)
                    {
                        result = dbContext.Insert(tableName, data);
                        
                        if (!result)
                        {
                            Debug.LogWarning($"Can not write metadata ({data}) into database file: {tableName}!");
                        }
                    }
                }

                // Copy metadata entity database file.
                CopyDatabaseFile(dbFolderPath, dbLocalAddress, tableName);
            }
        }

        #endregion Methods

        #region Structs

        private struct MetadataEntityRawInfo
        {
            #region Constructors

            public MetadataEntityRawInfo(string comment, string propertyType, string propertyName, params object[] parameters)
            {
                Comment = comment;
                PropertyType = propertyType;
                PropertyName = propertyName;
                Parameters = parameters;
            }

            #endregion Constructors

            #region Properties

            public string Comment
            {
                get;
            }

            public object[] Parameters
            {
                get;
            }

            public string PropertyName
            {
                get;
            }

            public string PropertyType
            {
                get;
            }

            #endregion Properties
        }

        #endregion Structs
    }
}
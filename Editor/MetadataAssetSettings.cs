// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System.IO;
using JetBrains.Annotations;
using ReSharp.Security.Cryptography;
using UniSharper;
using UnityEditor;
using UnityEngine;

namespace UniSharperEditor.Data.Metadata
{
    /// <summary>
    /// Class used to get and set the metadata settings object. Implements the <see cref="UniSharperEditor.SettingsScriptableObject"/>
    /// </summary>
    /// <seealso cref="UniSharperEditor.SettingsScriptableObject"/>
    public class MetadataAssetSettings : SettingsScriptableObject
    {
        #region Fields

        private const string MetadataFolderName = "Metadata";

        private const string MetadataPersistentStoresFolderName = "Data";

        private static readonly string ExcelWorkbookFilesFolderPathPrefKeyFormat =
            $"{CryptoUtility.Md5HashEncrypt(Directory.GetCurrentDirectory(), false)}.{typeof(MetadataAssetSettings).FullName}.excelWorkbookFilesFolderPath";

        private static readonly string MetadataFolderPath = PathUtility.UnifyToAltDirectorySeparatorChar(Path.Combine(EditorEnvironment.AssetsFolderName, MetadataFolderName));

        private static readonly string SettingsAssetPath = $"{MetadataFolderPath}/{nameof(MetadataAssetSettings)}.asset";

        [ReadOnlyField]
        [SerializeField]
        private bool dataEncryptionAndDecryption;

        [ReadOnlyField]
        [SerializeField]
        private int entityDataStartingRowIndex = 3;

        [ReadOnlyField]
        [SerializeField]
        private int entityPropertyCommentRowIndex;

        [ReadOnlyField]
        [SerializeField]
        private int entityPropertyNameRowIndex = 2;

        [ReadOnlyField]
        [SerializeField]
        private int entityPropertyTypeRowIndex = 1;

        [ReadOnlyField]
        [SerializeField]
        private string entityScriptNamespace;

        [ReadOnlyField]
        [SerializeField]
        private string entityScriptsStorePath = PathUtility.UnifyToAltDirectorySeparatorChar(Path.Combine(EditorEnvironment.AssetsFolderName, EditorEnvironment.DefaultScriptsFolderName));

        [ReadOnlyField]
        [SerializeField]
        private string metadataPersistentStorePath = PathUtility.UnifyToAltDirectorySeparatorChar(Path.Combine(MetadataFolderPath, MetadataPersistentStoresFolderName));

        #endregion Fields

        #region Properties

        internal bool DataEncryptionAndDecryption
        {
            get => dataEncryptionAndDecryption;
            set
            {
                if (dataEncryptionAndDecryption.Equals(value))
                    return;

                dataEncryptionAndDecryption = value;
                Save();
            }
        }

        internal int EntityDataStartingRowIndex
        {
            get => entityDataStartingRowIndex;
            set
            {
                if (entityDataStartingRowIndex.Equals(value))
                    return;
                entityDataStartingRowIndex = value;
                Save();
            }
        }

        internal int EntityPropertyCommentRowIndex
        {
            get => entityPropertyCommentRowIndex;
            set
            {
                if (entityPropertyCommentRowIndex.Equals(value))
                    return;
                entityPropertyCommentRowIndex = value;
                Save();
            }
        }

        internal int EntityPropertyNameRowIndex
        {
            get => entityPropertyNameRowIndex;
            set
            {
                if (entityPropertyNameRowIndex.Equals(value))
                    return;
                entityPropertyNameRowIndex = value;
                Save();
            }
        }

        internal int EntityPropertyTypeRowIndex
        {
            get => entityPropertyTypeRowIndex;
            set
            {
                if (entityPropertyTypeRowIndex.Equals(value))
                    return;
                entityPropertyTypeRowIndex = value;
                Save();
            }
        }

        internal string EntityScriptNamespace
        {
            get => entityScriptNamespace;
            set
            {
                if (entityScriptNamespace.Equals(value))
                    return;

                entityScriptNamespace = value;
                Save();
            }
        }

        internal string EntityScriptsStorePath
        {
            get => entityScriptsStorePath;
            set
            {
                if (entityScriptsStorePath.Equals(value))
                    return;

                entityScriptsStorePath = value;
                Save();
            }
        }

        internal string ExcelWorkbookFilesFolderPath
        {
            get
            {
                var key = string.Format(ExcelWorkbookFilesFolderPathPrefKeyFormat, PlayerSettings.productName);
                return EditorPrefs.GetString(key, string.Empty);
            }
            set
            {
                if (ExcelWorkbookFilesFolderPath.Equals(value))
                    return;

                var key = string.Format(ExcelWorkbookFilesFolderPathPrefKeyFormat, PlayerSettings.productName);
                EditorPrefs.SetString(key, value);
            }
        }

        internal string MetadataPersistentStorePath
        {
            get => metadataPersistentStorePath;
            set
            {
                if (metadataPersistentStorePath.Equals(value))
                    return;

                metadataPersistentStorePath = value;
                Save();
            }
        }

        #endregion Properties

        #region Methods

        internal static MetadataAssetSettings Create()
        {
            MetadataAssetSettings settings;

            if (File.Exists(SettingsAssetPath))
            {
                settings = Load();
            }
            else
            {
                settings = CreateInstance<MetadataAssetSettings>();
                CreateMetadataAssetsRootFolder();
                CreateMetadataPersistentStoreFolder(settings);
                CreateEntityScriptsStoreFolder(settings);
                AssetDatabase.CreateAsset(settings, SettingsAssetPath);
            }

            return settings;
        }

        internal static void CreateEntityScriptsStoreFolder(MetadataAssetSettings settings)
        {
            if (Directory.Exists(settings.EntityScriptsStorePath))
                return;
            Directory.CreateDirectory(settings.EntityScriptsStorePath);
            AssetDatabase.ImportAsset(settings.EntityScriptsStorePath);
        }

        internal static void CreateMetadataAssetsRootFolder()
        {
            if (!Directory.Exists(MetadataFolderPath))
            {
                AssetDatabase.CreateFolder("Assets", MetadataFolderName);
            }
        }

        internal static void CreateMetadataPersistentStoreFolder(MetadataAssetSettings settings)
        {
            if (Directory.Exists(settings.MetadataPersistentStorePath))
                return;
            Directory.CreateDirectory(settings.MetadataPersistentStorePath);
            AssetDatabase.ImportAsset(settings.MetadataPersistentStorePath);
        }

        internal static MetadataAssetSettings Load() => File.Exists(SettingsAssetPath) ? AssetDatabase.LoadAssetAtPath<MetadataAssetSettings>(SettingsAssetPath) : null;

        [UsedImplicitly]
        private void OnEnable()
        {
            if (string.IsNullOrEmpty(entityScriptNamespace))
            {
                entityScriptNamespace = $"{(string.IsNullOrEmpty(PlayerSettings.productName) ? "Project" : PlayerSettings.productName)}.Metadata.Entities";
            }
        }

        #endregion Methods
    }
}
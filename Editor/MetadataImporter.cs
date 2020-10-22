// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEditorUtility = UnityEditor.EditorUtility;

namespace UniSharperEditor.Data.Metadata
{
    internal class MetadataImporter
    {
        #region Fields

        private const float LabelWidth = 275f;

        private const int MaxIntValue = 5;

        private const string ScriptsGeneratedPrefKey = "UniSharperEditor.Data.Metadata.MetadataImporterscriptsGenerated";

        private readonly MetadataAssetSettings settings;

        #endregion Fields

        #region Constructors

        internal MetadataImporter(MetadataAssetSettings settings)
        {
            this.settings = settings;
        }

        #endregion Constructors

        #region Methods

        internal void DrawEditorGui()
        {
            if (settings == null)
            {
                return;
            }

            GUILayout.BeginVertical();
            
            // Import Settings
            GUILayout.Label("Import Settings", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.BeginVertical();

            // Excel Sheets Folder Path
            if (!string.IsNullOrEmpty(settings.ExcelWorkbookFilesFolderPath) && !Directory.Exists(settings.ExcelWorkbookFilesFolderPath))
            {
                settings.ExcelWorkbookFilesFolderPath = string.Empty;
            }
            settings.ExcelWorkbookFilesFolderPath = EditorGUILayoutUtility.FolderField(new GUIContent("Excel Workbook Files Folder Path", 
                "The folder path where to locate excel workbook files."), settings.ExcelWorkbookFilesFolderPath, "Excel Workbook Files Folder Path", string.Empty, string.Empty, LabelWidth);

            // Metadata Persistent Store Path
            string metadataPersistentStoreAbsolutePath = EditorGUILayoutUtility.FolderField(new GUIContent("Metadata Persistent Store Path", 
                "The folder path where to store metadata."), settings.MetadataPersistentStorePath, "Metadata Persistent Store Path", settings.MetadataPersistentStorePath, string.Empty, LabelWidth);

            if (EditorPath.IsAssetPath(metadataPersistentStoreAbsolutePath))
            {
                settings.MetadataPersistentStorePath = EditorPath.ConvertToAssetPath(metadataPersistentStoreAbsolutePath);
            }
            else
            {
                UnityEditorUtility.DisplayDialog("Invalid Path", 
                    "The 'Metadata Persistent Store Path' you choose is invalid path, please select the folder in the project!", "OK");
            }

            // Metadata Entity Scripts Store Path
            string entityScriptsStoreAbsolutePath = EditorGUILayoutUtility.FolderField(new GUIContent("Entity Scripts Store Path", 
                "The folder path where to store metadata entity scripts."), settings.EntityScriptsStorePath, "Entity Scripts Store Path", settings.EntityScriptsStorePath, string.Empty, LabelWidth);

            if (EditorPath.IsAssetPath(entityScriptsStoreAbsolutePath))
            {
                settings.EntityScriptsStorePath = EditorPath.ConvertToAssetPath(entityScriptsStoreAbsolutePath);
            }
            else
            {
                UnityEditorUtility.DisplayDialog("Invalid Path", 
                    "The 'Entity Scripts Store Path' you choose is invalid path, please select the folder in the project!", "OK");
            }

            // Metadata Entity Namespace
            using (new EditorGUIFieldScope(LabelWidth))
            {
                settings.EntityScriptNamespace = EditorGUILayout.TextField(new GUIContent("Entity Script Namespace", 
                    "The namespace of entity script."), settings.EntityScriptNamespace);
            }

            // Metadata Entity Property Comment Row Index
            using (new EditorGUIFieldScope(LabelWidth))
            {
                settings.EntityPropertyCommentRowIndex = EditorGUILayout.IntSlider(new GUIContent("Entity Property Comment Definition Row Index", 
                    "The row index of entity property comment definition."), settings.EntityPropertyCommentRowIndex, 0, MaxIntValue);
            }

            // Metadata Entity Property Type Row Index
            using (new EditorGUIFieldScope(LabelWidth))
            {
                settings.EntityPropertyTypeRowIndex = EditorGUILayout.IntSlider(new GUIContent("Entity Property Type Definition Row Index", 
                    "The row index of entity property type definition."), settings.EntityPropertyTypeRowIndex, 0, MaxIntValue);
            }

            // Metadata Entity Property Name Row Index
            using (new EditorGUIFieldScope(LabelWidth))
            {
                settings.EntityPropertyNameRowIndex = EditorGUILayout.IntSlider(new GUIContent("Entity Property Name Definition Row Index", 
                    "The row index of entity property name definition."), settings.EntityPropertyNameRowIndex, 0, MaxIntValue);
            }

            // Metadata Entity Property Value Starting Row Index
            using (new EditorGUIFieldScope(LabelWidth))
            {
                settings.EntityDataStartingRowIndex = EditorGUILayout.IntSlider(new GUIContent("Entity Data Starting Row Index", 
                    "The row index and after will be entity data definitions."), settings.EntityDataStartingRowIndex, 0, MaxIntValue);
            }
            
            GUILayout.EndVertical();
            GUILayout.Space(10);
            GUILayout.EndHorizontal();

            GUILayout.Space(20);
            
            // Other Settings
            GUILayout.Label("Other Settings", EditorStyles.boldLabel);
            
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.BeginVertical();
            
            // Data encryption/decryption
            using (new EditorGUIFieldScope(LabelWidth))
            {
                settings.DataEncryptionAndDecryption = EditorGUILayout.Toggle(new GUIContent("Data Encryption/Decryption", 
                    "Generate database file with encryption, and load metadata with decryption."), settings.DataEncryptionAndDecryption);
            }
            
            GUILayout.EndVertical();
            GUILayout.Space(10);
            GUILayout.EndHorizontal();
            

            GUILayout.Space(20);

            using (new EditorGUIFieldScope(LabelWidth))
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Build Metadata Assets", GUILayout.Width(LabelWidth), GUILayout.Height(25)))
                {
                    BuildAssets();
                    GUIUtility.ExitGUI();
                }
                GUILayout.FlexibleSpace();
            }

            GUILayout.Space(20);
            
            GUILayout.EndVertical();
        }

        private static void BuildAssets()
        {
            EditorUtility.ClearConsole();

            if (UnityEditorUtility.scriptCompilationFailed) 
                return;
            
            try
            {
                var result = MetadataAssetUtility.GenerateMetadataEntityScripts();
                EditorPrefs.SetBool(ScriptsGeneratedPrefKey, result);

                if (result)
                {
                    UnityEditorUtility.DisplayProgressBar("Hold on...", "Compiling metadata entity scripts...", 0f);
                }
                else
                {
                    UnityEditorUtility.DisplayDialog("Error", "Failed to generate metadata entity scripts!", "OK");
                }
            }
            catch (System.Exception)
            {
                UnityEditorUtility.ClearProgressBar();
                throw;
            }
        }

        [DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            var scriptsGenerated = EditorPrefs.GetBool(ScriptsGeneratedPrefKey);
            if (!scriptsGenerated)
                return;
            
            UnityEditorUtility.ClearProgressBar();
            EditorPrefs.SetBool(ScriptsGeneratedPrefKey, false);

            if (!UnityEditorUtility.scriptCompilationFailed)
            {
                try
                {
                    var result = MetadataAssetUtility.CreateMetadataDatabaseFiles();

                    if (result)
                    {
                        UnityEditorUtility.DisplayDialog("Success", "Build success!", "OK");
                    }
                    else
                    {
                        UnityEditorUtility.DisplayDialog("Error", "Failed to create metadata database files!", "OK");
                    }
                }
                catch (System.Exception)
                {
                    UnityEditorUtility.ClearProgressBar();
                    throw;
                }
            }
            else
            {
                UnityEditorUtility.DisplayDialog("Error", "Failed to compile metadata entity scripts!", "OK");
            }
        }

        #endregion Methods
    }
}
// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace UniSharperEditor.Data.Metadata
{
    internal class MetadataImporter
    {
        private const float LabelWidth = 275f;

        private const float Padding = 10;

        private const int MaxIntValue = 5;
        
        private static readonly string ScriptsGeneratedPrefKey = $"{typeof(MetadataImporter).FullName}.MetadataImporterScriptsGenerated";
        
        private readonly MetadataAssetSettings settings;
        
        private static GUIStyle titleBoxGUIStyle; // GUIStyle used to draw the results of the search
        
        public static GUIStyle BoxGUIStyle
        {
            get
            {
                if( titleBoxGUIStyle == null )
                {
                    titleBoxGUIStyle = new GUIStyle( EditorStyles.helpBox )
                    {
                        alignment = TextAnchor.MiddleLeft,
                        margin = new RectOffset { top = 8, bottom = 8 },
                        padding = new RectOffset { left = 10, right = 10 },
                        font = EditorStyles.label.font,
                        fontSize = 14,
                        richText = true
                    };

                    var textColor = GUI.skin.button.normal.textColor;
                    titleBoxGUIStyle.normal.textColor = textColor;
                    titleBoxGUIStyle.hover.textColor = textColor;
                    titleBoxGUIStyle.focused.textColor = textColor;
                    titleBoxGUIStyle.active.textColor = textColor;
                }

                return titleBoxGUIStyle;
            }
        }
        
        public static readonly GUILayoutOption TitleBoxHeight = GUILayout.Height(30);

        internal MetadataImporter(MetadataAssetSettings settings)
        {
            this.settings = settings;
        }
        
        [DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            var scriptsGenerated = EditorPrefs.GetBool(ScriptsGeneratedPrefKey);
            if (!scriptsGenerated)
                return;

            EditorUtility.ClearProgressBar();
            EditorPrefs.SetBool(ScriptsGeneratedPrefKey, false);

            if (!EditorUtility.scriptCompilationFailed)
            {
                try
                {
                    var result = MetadataAssetUtility.CreateMetadataDatabaseFiles();

                    if (result)
                    {
                        MetadataAssetUtility.SaveExcelWorkbookFileHashMap();

                        if (EditorUtility.DisplayDialog("Success", "Build success!", "OK"))
                            EditorUtility.ClearProgressBar();
                    }
                    else
                    {
                        if (EditorUtility.DisplayDialog("Error", "Failed to create metadata database files!", "OK"))
                            EditorUtility.ClearProgressBar();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            else
            {
                if (EditorUtility.DisplayDialog("Error", "Failed to compile metadata entity scripts!", "OK"))
                    EditorUtility.ClearProgressBar();
            }
        }

        private static void DrawTitleLabel(string text)
        {
            GUILayout.Box(text, BoxGUIStyle, TitleBoxHeight);
        }

        private static void BuildAssets()
        {
            UniEditorUtility.ClearConsole();

            if (EditorUtility.scriptCompilationFailed)
                return;

            try
            {
                var result = MetadataAssetUtility.GenerateMetadataEntityScripts();
                EditorPrefs.SetBool(ScriptsGeneratedPrefKey, result);

                if (result)
                {
                    EditorUtility.DisplayProgressBar("Hold on...", "Compiling metadata entity scripts...", 0f);
                }
                else
                {
                    if (EditorUtility.DisplayDialog("Error", "Failed to generate metadata entity scripts!", "OK"))
                        EditorUtility.ClearProgressBar();
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        internal void DrawEditorGUI()
        {
            if (settings == null)
                return;

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.Space(Padding);
                    
                EditorGUILayout.BeginVertical();
                {
                    // Import Settings
                    DrawImportSettingsFields();
                    
                    EditorGUILayout.Space(5);
                    
                    // Other Settings
                    DrawOtherSettingsFields();
                    
                    EditorGUILayout.Space(10);
                    
                    // Build Assets Button
                    DrawBuildAssetsButton();
                }
                EditorGUILayout.EndVertical();
                
                EditorGUILayout.Space(Padding);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawImportSettingsFields()
        {
            const string title = "Import Settings";
            DrawTitleLabel(title);

            // Excel Sheets Folder Path
            if (!string.IsNullOrEmpty(settings.ExcelWorkbookFilesFolderPath) && !Directory.Exists(settings.ExcelWorkbookFilesFolderPath))
                settings.ExcelWorkbookFilesFolderPath = string.Empty;
            
            settings.ExcelWorkbookFilesFolderPath = UniEditorGUILayout.FolderField(new GUIContent("Excel Workbook Files Folder Path", 
                    "The folder path where to locate excel workbook files."), settings.ExcelWorkbookFilesFolderPath, "Excel Workbook Files Folder Path", 
                string.Empty, string.Empty, LabelWidth);

            // Metadata Persistent Store Path
            var metadataPersistentStoreAbsolutePath = UniEditorGUILayout.FolderField(new GUIContent("Metadata Persistent Store Path",
                "The folder path where to store metadata."), settings.MetadataPersistentStorePath, "Metadata Persistent Store Path", settings.MetadataPersistentStorePath, string.Empty, LabelWidth);
            
            if (EditorPath.IsAssetPath(metadataPersistentStoreAbsolutePath))
            {
                settings.MetadataPersistentStorePath = EditorPath.ConvertToAssetPath(metadataPersistentStoreAbsolutePath);
            }
            else
            {
                Debug.LogError("The 'Metadata Persistent Store Path' you choose is invalid path, please select the folder in the project!");
            }
            
            // Metadata Entity Scripts Store Path
            var entityScriptsStoreAbsolutePath = UniEditorGUILayout.FolderField(new GUIContent("Entity Scripts Store Path",
                "The folder path where to store metadata entity scripts."), settings.EntityScriptsStorePath, "Entity Scripts Store Path", settings.EntityScriptsStorePath, string.Empty, LabelWidth);
            
            if (EditorPath.IsAssetPath(entityScriptsStoreAbsolutePath))
            {
                settings.EntityScriptsStorePath = EditorPath.ConvertToAssetPath(entityScriptsStoreAbsolutePath);
            }
            else
            {
                Debug.LogError("The 'Entity Scripts Store Path' you choose is invalid path, please select the folder in the project!");
            }
            
            // Metadata Entity Namespace
            using (new UniEditorGUILayout.FieldScope(LabelWidth))
            {
                settings.EntityScriptNamespace = EditorGUILayout.TextField(new GUIContent("Entity Script Namespace",
                    "The namespace of entity script."), settings.EntityScriptNamespace);
            }
            
            // Metadata Entity Property Comment Row Index
            using (new UniEditorGUILayout.FieldScope(LabelWidth))
            {
                settings.EntityPropertyCommentRowIndex = EditorGUILayout.IntSlider(new GUIContent("Entity Property Comment Definition Row Index",
                    "The row index of entity property comment definition."), settings.EntityPropertyCommentRowIndex, 0, MaxIntValue);
            }
            
            // Metadata Entity Property Type Row Index
            using (new UniEditorGUILayout.FieldScope(LabelWidth))
            {
                settings.EntityPropertyTypeRowIndex = EditorGUILayout.IntSlider(new GUIContent("Entity Property Type Definition Row Index",
                    "The row index of entity property type definition."), settings.EntityPropertyTypeRowIndex, 0, MaxIntValue);
            }
            
            // Metadata Entity Property Name Row Index
            using (new UniEditorGUILayout.FieldScope(LabelWidth))
            {
                settings.EntityPropertyNameRowIndex = EditorGUILayout.IntSlider(new GUIContent("Entity Property Name Definition Row Index",
                    "The row index of entity property name definition."), settings.EntityPropertyNameRowIndex, 0, MaxIntValue);
            }
            
            // Metadata Entity Property Value Starting Row Index
            using (new UniEditorGUILayout.FieldScope(LabelWidth))
            {
                settings.EntityDataStartingRowIndex = EditorGUILayout.IntSlider(new GUIContent("Entity Data Starting Row Index",
                    "The row index and after will be entity data definitions."), settings.EntityDataStartingRowIndex, 0, MaxIntValue);
            }
        }

        private void DrawOtherSettingsFields()
        {
            const string title = "Other Settings";
            DrawTitleLabel(title);
            
            // Data encryption/decryption
            using (new UniEditorGUILayout.FieldScope(LabelWidth))
            {
                settings.DataEncryptionAndDecryption = EditorGUILayout.Toggle(new GUIContent("Data Encryption/Decryption",
                    "Generate database file with encryption, and load metadata with decryption."), settings.DataEncryptionAndDecryption);
            }
            
            // Delete or not delete redundant metadata and entity scripts
            using (new UniEditorGUILayout.FieldScope(LabelWidth))
            {
                settings.DeleteRedundantMetadataAndEntityScripts = EditorGUILayout.Toggle(new GUIContent("Delete Redundant Metadata?",
                    "Delete or not delete redundant metadata and entity scripts."), settings.DeleteRedundantMetadataAndEntityScripts);
            }
        }

        private void DrawBuildAssetsButton()
        {
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Build Metadata Assets", GUILayout.Width(LabelWidth), GUILayout.Height(25)))
                    BuildAssets();
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
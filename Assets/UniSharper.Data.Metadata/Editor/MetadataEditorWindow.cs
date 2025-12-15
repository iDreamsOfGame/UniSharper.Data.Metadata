// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using UnityEditor;
using UnityEngine;

namespace UniSharperEditor.Data.Metadata
{
    internal abstract class MetadataEditorWindow : EditorWindow
    {
        private MetadataAssetSettings settings;

        protected MetadataAssetSettings Settings
        {
            get
            {
                if (settings == null)
                {
                    settings = MetadataAssetSettings.Load();
                }

                return settings;
            }
        }

        protected virtual void DrawGUIWithoutSettings()
        {
            GUILayout.Space(50);
            if (GUILayout.Button("Create Metadata Settings"))
            {
                settings = MetadataAssetSettings.Create();
            }
            GUILayout.Space(20);
            GUILayout.BeginHorizontal();
            GUILayout.Space(50);
            GUI.skin.label.wordWrap = true;
            GUILayout.Label("Click the \"Create\" button above to start using Metadata.  Once you begin, the Metadata system will save some assets to your project to keep up with its data");
            GUILayout.Space(50);
            GUILayout.EndHorizontal();
        }

        protected virtual void DrawGUIWithSettings()
        {
        }

        private void OnGUI()
        {
            if (Settings == null)
            {
                DrawGUIWithoutSettings();
            }
            else
            {
                DrawGUIWithSettings();
            }
        }
    }
}
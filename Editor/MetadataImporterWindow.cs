// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using UnityEditor;
using UnityEngine;

namespace UniSharperEditor.Data.Metadata
{
    internal class MetadataImporterWindow : MetadataEditorWindow
    {
        #region Fields

        private MetadataImporter importer;

        #endregion Fields

        #region Methods

        [MenuItem("UniSharper/Metadata Management/Metadata Importer", false, 1)]
        internal static void ShowWindow()
        {
            var window = GetWindow<MetadataImporterWindow>("Metadata Importer", true);
            window.minSize = new Vector2(840, 300);
            window.Show();
        }

        protected override void DrawGUIWithSettings()
        {
            if (importer == null)
            {
                importer = new MetadataImporter(Settings);
            }

            importer.DrawEditorGUI();
        }

        #endregion Methods
    }
}
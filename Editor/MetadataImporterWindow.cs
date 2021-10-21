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
            const float width = 840;
            const float height = 300;
            const string title = "Metadata Importer";
            var position = new Vector2((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f);
            var size = new Vector2(width, height);
            var rect = new Rect(position, size);
            var window = GetWindowWithRect<MetadataImporterWindow>(rect, true, title);
            window.minSize = window.maxSize = size;
            window.Show();
        }

        protected override void DrawGUIWithSettings()
        {
            importer ??= new MetadataImporter(Settings);
            importer.DrawEditorGUI();
        }

        #endregion Methods
    }
}
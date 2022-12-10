// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using UnityEditor;
using UnityEngine;

namespace UniSharperEditor.Data.Metadata
{
    internal class MetadataImporterWindow : MetadataEditorWindow
    {
        private static readonly Vector2Int Size = new(840, 375);

        private MetadataImporter importer;

        [MenuItem("UniSharper/Metadata Management/Metadata Importer", false, 1)]
        internal static void ShowWindow()
        {
            const string title = "Metadata Importer";
            var position = new Vector2((Screen.width - Size.x) * 0.5f, (Screen.height - Size.y) * 0.5f);
            var rect = new Rect(position, Size);
            var window = GetWindowWithRect<MetadataImporterWindow>(rect, true, title);
            window.minSize = window.maxSize = Size;
            window.Show();
        }

        protected override void DrawGUIWithSettings()
        {
            importer ??= new MetadataImporter(Settings);
            importer.DrawEditorGUI();
        }
    }
}
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
            GetWindowWithRect<MetadataImporterWindow>(new Rect(0, 0, 850, 300), true, "Metadata Importer").Show();
        }

        protected override void DrawGUIWithSettings()
        {
            if (importer == null)
            {
                importer = new MetadataImporter(Settings);
            }

            importer.DrawEditorGui();
        }

        #endregion Methods
    }
}
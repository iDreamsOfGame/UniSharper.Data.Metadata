// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using UnityEditor;

namespace UniSharperEditor.Data.Metadata
{
    internal static class InternalEditorUtility
    {
        public static void ClearProgressBar()
        {
            EditorUtility.ClearProgressBar();
            SceneView.RepaintAll();
        }
    }
}
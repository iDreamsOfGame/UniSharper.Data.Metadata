// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using UnityEngine;

namespace UniSharperEditor.Data.Metadata.EntityScriptPropertyStringEditors
{
    internal class UnityColor32PropertyStringEditor : UnityTypePropertyStringEditor<Color32>
    {
        protected override string ValuePropertyTypeName => PropertyTypeNames.ByteArray;

        protected override int NumberOfParameters => 4;
    }
}
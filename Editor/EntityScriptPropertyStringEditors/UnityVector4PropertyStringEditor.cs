// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using UnityEngine;

namespace UniSharperEditor.Data.Metadata.EntityScriptPropertyStringEditors
{
    internal class UnityVector4PropertyStringEditor : UnityTypePropertyStringEditor<Vector4>
    {
        protected override string ValuePropertyTypeName => PropertyTypeNames.SingleArray;
        
        protected override int NumberOfParameters => 4;
    }
}
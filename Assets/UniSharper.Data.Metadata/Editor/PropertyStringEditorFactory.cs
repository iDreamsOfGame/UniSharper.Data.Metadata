// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using ReSharp.Patterns.Factory;
using UniSharperEditor.Data.Metadata.PropertyStringEditors;

namespace UniSharperEditor.Data.Metadata
{
    internal class PropertyStringEditorFactory : CachingFactoryTemplate<PropertyStringEditorFactory, string, IPropertyStringEditor>
    {
        private static readonly Dictionary<string, Type> PropertyTypeStringEditorTypeMap = new()
        {
            { PropertyTypeNames.Enum, typeof(EnumPropertyStringEditor) },
            { PropertyTypeNames.UnityVector2, typeof(UnityTypePropertyStringEditor) },
            { PropertyTypeNames.UnityVector2Int, typeof(UnityTypePropertyStringEditor) },
            { PropertyTypeNames.UnityVector3, typeof(UnityTypePropertyStringEditor) },
            { PropertyTypeNames.UnityVector3Int, typeof(UnityTypePropertyStringEditor) },
            { PropertyTypeNames.UnityVector4, typeof(UnityTypePropertyStringEditor) },
            { PropertyTypeNames.UnityRangeInt, typeof(UnityTypePropertyStringEditor) },
            { PropertyTypeNames.UnityQuaternion, typeof(UnityTypePropertyStringEditor) },
            { PropertyTypeNames.UnityRect, typeof(UnityTypePropertyStringEditor) },
            { PropertyTypeNames.UnityRectInt, typeof(UnityTypePropertyStringEditor) },
            { PropertyTypeNames.UnityColor, typeof(UnityTypePropertyStringEditor) },
            { PropertyTypeNames.UnityColor32, typeof(UnityTypePropertyStringEditor) },
            { PropertyTypeNames.UnityVector2Array, typeof(UnityTypeArrayPropertyStringEditor) },
            { PropertyTypeNames.UnityVector2IntArray, typeof(UnityTypeArrayPropertyStringEditor) },
            { PropertyTypeNames.UnityVector3Array, typeof(UnityTypeArrayPropertyStringEditor) },
            { PropertyTypeNames.UnityVector3IntArray, typeof(UnityTypeArrayPropertyStringEditor) },
            { PropertyTypeNames.UnityVector4Array, typeof(UnityTypeArrayPropertyStringEditor) },
            { PropertyTypeNames.UnityRangeIntArray, typeof(UnityTypeArrayPropertyStringEditor) },
            { PropertyTypeNames.UnityQuaternionArray, typeof(UnityTypeArrayPropertyStringEditor) },
            { PropertyTypeNames.UnityRectArray, typeof(UnityTypeArrayPropertyStringEditor) },
            { PropertyTypeNames.UnityRectIntArray, typeof(UnityTypeArrayPropertyStringEditor) },
            { PropertyTypeNames.UnityColorArray, typeof(UnityTypeArrayPropertyStringEditor) },
            { PropertyTypeNames.UnityColor32Array, typeof(UnityTypeArrayPropertyStringEditor) }
        };

        private PropertyStringEditorFactory()
            : base(PropertyTypeStringEditorTypeMap)
        {
        }
    }
}
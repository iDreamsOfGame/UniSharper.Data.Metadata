// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using ReSharp.Patterns.Factory;
using UniSharperEditor.Data.Metadata.PropertyRawInfoEditors;

namespace UniSharperEditor.Data.Metadata
{
    internal class PropertyRawInfoEditorFactory : CachingFactoryTemplate<PropertyRawInfoEditorFactory, string, IPropertyRawInfoEditor>
    {
        private static readonly Dictionary<string, Type> PropertyTypeEditorTypeMap = new()
        {
            { PropertyTypeNames.Enum, typeof(EnumTypeRawInfoEditor) },
            { PropertyTypeNames.UnityVector2, typeof(UnityVector2RawInfoEditor) },
            { PropertyTypeNames.UnityVector2Int, typeof(UnityVector2IntRawInfoEditor) },
            { PropertyTypeNames.UnityVector3, typeof(UnityVector3RawInfoEditor) },
            { PropertyTypeNames.UnityVector3Int, typeof(UnityVector3IntRawInfoEditor) },
            { PropertyTypeNames.UnityVector4, typeof(UnityVector4RawInfoEditor) },
            { PropertyTypeNames.UnityRangeInt, typeof(UnityRangeIntRawInfoEditor) },
            { PropertyTypeNames.UnityQuaternion, typeof(UnityQuaternionRawInfoEditor) },
            { PropertyTypeNames.UnityRect, typeof(UnityRectRawInfoEditor) },
            { PropertyTypeNames.UnityRectInt, typeof(UnityRectIntRawInfoEditor) },
            { PropertyTypeNames.UnityColor, typeof(UnityColorRawInfoEditor) },
            { PropertyTypeNames.UnityColor32, typeof(UnityColor32RawInfoEditor) },
            { PropertyTypeNames.UnityVector2Array, typeof(UnityVector2RawInfoEditor) },
            { PropertyTypeNames.UnityVector2IntArray, typeof(UnityVector2IntRawInfoEditor) },
            { PropertyTypeNames.UnityVector3Array, typeof(UnityVector3RawInfoEditor) },
            { PropertyTypeNames.UnityVector3IntArray, typeof(UnityVector3IntRawInfoEditor) },
            { PropertyTypeNames.UnityVector4Array, typeof(UnityVector4RawInfoEditor) },
            { PropertyTypeNames.UnityRangeIntArray, typeof(UnityRangeIntRawInfoEditor) },
            { PropertyTypeNames.UnityQuaternionArray, typeof(UnityQuaternionRawInfoEditor) },
            { PropertyTypeNames.UnityRectArray, typeof(UnityRectRawInfoEditor) },
            { PropertyTypeNames.UnityRectIntArray, typeof(UnityRectIntRawInfoEditor) },
            { PropertyTypeNames.UnityColorArray, typeof(UnityColorRawInfoEditor) },
            { PropertyTypeNames.UnityColor32Array, typeof(UnityColor32RawInfoEditor) }
        };

        private PropertyRawInfoEditorFactory()
            : base(PropertyTypeEditorTypeMap)
        {
        }
    }
}
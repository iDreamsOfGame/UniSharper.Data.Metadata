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
            { PropertyTypeNames.UnityVector2, typeof(UnityTypeRawInfoEditor) },
            { PropertyTypeNames.UnityVector2Int, typeof(UnityTypeRawInfoEditor) },
            { PropertyTypeNames.UnityVector3, typeof(UnityTypeRawInfoEditor) },
            { PropertyTypeNames.UnityVector3Int, typeof(UnityTypeRawInfoEditor) },
            { PropertyTypeNames.UnityVector4, typeof(UnityTypeRawInfoEditor) },
            { PropertyTypeNames.UnityRangeInt, typeof(UnityTypeRawInfoEditor) },
            { PropertyTypeNames.UnityQuaternion, typeof(UnityTypeRawInfoEditor) },
            { PropertyTypeNames.UnityRect, typeof(UnityTypeRawInfoEditor) },
            { PropertyTypeNames.UnityRectInt, typeof(UnityTypeRawInfoEditor) },
            { PropertyTypeNames.UnityColor, typeof(UnityTypeRawInfoEditor) },
            { PropertyTypeNames.UnityColor32, typeof(UnityTypeRawInfoEditor) }
        };

        private PropertyRawInfoEditorFactory()
            : base(PropertyTypeEditorTypeMap)
        {
        }
    }
}
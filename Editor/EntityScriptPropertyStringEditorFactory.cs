// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using ReSharp.Patterns;
using UniSharperEditor.Data.Metadata.EntityScriptPropertyStringEditors;

namespace UniSharperEditor.Data.Metadata
{
    internal class EntityScriptPropertyStringEditorFactory : CachingFactoryTemplate<EntityScriptPropertyStringEditorFactory, string, IEntityScriptPropertyStringEditor>
    {
        private static readonly Dictionary<string, Type> PropertyTypeStringEditorTypeMap = new()
        {
            { PropertyTypeNames.Enum, typeof(EnumPropertyStringEditor) },
            { PropertyTypeNames.UnityVector2, typeof(UnityVector2PropertyStringEditor) },
            { PropertyTypeNames.UnityVector2Int, typeof(UnityVector2IntPropertyStringEditor) },
            { PropertyTypeNames.UnityVector3, typeof(UnityVector3PropertyStringEditor) },
            { PropertyTypeNames.UnityVector3Int, typeof(UnityVector3IntPropertyStringEditor) },
            { PropertyTypeNames.UnityVector4, typeof(UnityVector4PropertyStringEditor) },
            { PropertyTypeNames.UnityRangeInt, typeof(UnityRangeIntPropertyStringEditor) },
            { PropertyTypeNames.UnityQuaternion, typeof(UnityQuaternionPropertyStringEditor) },
            { PropertyTypeNames.UnityRect, typeof(UnityRectPropertyStringEditor) },
            { PropertyTypeNames.UnityRectInt, typeof(UnityRectIntPropertyStringEditor) },
            { PropertyTypeNames.UnityColor, typeof(UnityColorPropertyStringEditor) },
            { PropertyTypeNames.UnityColor32, typeof(UnityColor32PropertyStringEditor) }
        };

        private EntityScriptPropertyStringEditorFactory()
            : base(PropertyTypeStringEditorTypeMap)
        {
        }
    }
}
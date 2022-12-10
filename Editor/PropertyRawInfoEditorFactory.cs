using System;
using System.Collections.Generic;
using System.Data;
using UniSharperEditor.Data.Metadata.EntityRawInfoEditors;

namespace UniSharperEditor.Data.Metadata
{
    internal static class PropertyRawInfoEditorFactory
    {
        private static readonly Dictionary<Type, IPropertyRawInfoEditor> EditorsCache = new();

        private static readonly Dictionary<string, Type> PropertyTypeEditorTypeMap = new()
        {
            { PropertyTypeNames.Enum, typeof(EnumPropertyRawInfoEditor) }
        };

        internal static IPropertyRawInfoEditor GetEditor(string typeString, MetadataAssetSettings settings, DataTable table)
        {
            if (string.IsNullOrEmpty(typeString))
                return null;

            var type = GetEditorType(typeString);

            if (type == null)
                return null;

            if (EditorsCache.TryGetValue(type, out var editor))
                return editor;

            editor = CreateEditor(typeString, settings, table);
            EditorsCache.Add(type, editor);
            return editor;
        }
        
        private static IPropertyRawInfoEditor CreateEditor(string typeString, MetadataAssetSettings settings, DataTable table)
        {
            var type = GetEditorType(typeString);
            return (IPropertyRawInfoEditor)type?.InvokeConstructor(new object[] { settings, table });
        }
        
        private static Type GetEditorType(string typeString) => 
            !string.IsNullOrEmpty(typeString) && PropertyTypeEditorTypeMap.TryGetValue(typeString, out var type) ? type : null;
    }
}
using System;
using System.Collections.Generic;
using UniSharperEditor.Data.Metadata.EntityScriptPropertyStringEditors;

namespace UniSharperEditor.Data.Metadata
{
    internal static class EntityScriptPropertyStringEditorFactory
    {
        private static readonly Dictionary<Type, IEntityScriptPropertyStringEditor> EditorsCache = new();

        private static readonly Dictionary<string, Type> PropertyTypeStringEditorTypeMap = new()
        {
            { PropertyTypeNames.Enum, typeof(EnumPropertyStringEditor) }
        };
        
        internal static IEntityScriptPropertyStringEditor GetEditor(string typeString)
        {
            if (string.IsNullOrEmpty(typeString))
                return null;

            var type = GetEditorType(typeString);

            if (type == null)
                return null;

            if (EditorsCache.TryGetValue(type, out var editor))
                return editor;

            editor = CreateEditor(typeString);
            EditorsCache.Add(type, editor);
            return editor;
        }
        
        private static IEntityScriptPropertyStringEditor CreateEditor(string typeString)
        {
            var type = GetEditorType(typeString);
            return (IEntityScriptPropertyStringEditor)type?.InvokeConstructor();
        }
        
        private static Type GetEditorType(string typeString) => 
            !string.IsNullOrEmpty(typeString) && PropertyTypeStringEditorTypeMap.TryGetValue(typeString, out var type) ? type : null;
    }
}
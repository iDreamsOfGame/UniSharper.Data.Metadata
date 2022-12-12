using UnityEngine;

namespace UniSharperEditor.Data.Metadata.EntityScriptPropertyStringEditors
{
    internal class UnityColor32PropertyStringEditor : UnityTypePropertyStringEditor<Color32>
    {
        protected override string ValuePropertyTypeName => PropertyTypeNames.ByteArray;

        protected override int NumberOfParameters => 4;
    }
}
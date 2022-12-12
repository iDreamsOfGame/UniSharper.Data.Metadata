using UnityEngine;

namespace UniSharperEditor.Data.Metadata.EntityScriptPropertyStringEditors
{
    internal class UnityRectIntPropertyStringEditor : UnityTypePropertyStringEditor<RectInt>
    {
        protected override string ValuePropertyTypeName => PropertyTypeNames.Int32Array;
        
        protected override int NumberOfParameters => 4;
    }
}
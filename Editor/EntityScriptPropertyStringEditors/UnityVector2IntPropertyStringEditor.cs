using UnityEngine;

namespace UniSharperEditor.Data.Metadata.EntityScriptPropertyStringEditors
{
    internal class UnityVector2IntPropertyStringEditor : UnityTypePropertyStringEditor<Vector2Int>
    {
        protected override string ValuePropertyTypeName => PropertyTypeNames.Int32Array;
        
        protected override int NumberOfParameters => 2;
    }
}
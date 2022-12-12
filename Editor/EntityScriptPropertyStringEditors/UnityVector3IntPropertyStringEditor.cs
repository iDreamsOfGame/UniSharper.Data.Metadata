using UnityEngine;

namespace UniSharperEditor.Data.Metadata.EntityScriptPropertyStringEditors
{
    internal class UnityVector3IntPropertyStringEditor : UnityTypePropertyStringEditor<Vector3Int>
    {
        protected override string ValuePropertyTypeName => PropertyTypeNames.Int32Array;
        
        protected override int NumberOfParameters => 3;
    }
}
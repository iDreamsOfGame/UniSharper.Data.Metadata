using UnityEngine;

namespace UniSharperEditor.Data.Metadata.EntityScriptPropertyStringEditors
{
    internal class UnityVector4PropertyStringEditor : UnityTypePropertyStringEditor<Vector4>
    {
        protected override string ValuePropertyTypeName => PropertyTypeNames.SingleArray;
        
        protected override int NumberOfParameters => 4;
    }
}
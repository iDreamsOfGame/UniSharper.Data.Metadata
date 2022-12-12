using UnityEngine;

namespace UniSharperEditor.Data.Metadata.EntityScriptPropertyStringEditors
{
    internal class UnityVector3PropertyStringEditor : UnityTypePropertyStringEditor<Vector3>
    {
        protected override string ValuePropertyTypeName => PropertyTypeNames.SingleArray;
        
        protected override int NumberOfParameters => 3;
    }
}
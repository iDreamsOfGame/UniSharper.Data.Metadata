using UnityEngine;

namespace UniSharperEditor.Data.Metadata.EntityScriptPropertyStringEditors
{
    internal class UnityQuaternionPropertyStringEditor : UnityTypePropertyStringEditor<Quaternion>
    {
        protected override string ValuePropertyTypeName => PropertyTypeNames.SingleArray;
        
        protected override int NumberOfParameters => 4;
    }
}
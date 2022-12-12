using UnityEngine;

namespace UniSharperEditor.Data.Metadata.EntityScriptPropertyStringEditors
{
    internal class UnityRectPropertyStringEditor : UnityTypePropertyStringEditor<Rect>
    {
        protected override string ValuePropertyTypeName => PropertyTypeNames.SingleArray;
        
        protected override int NumberOfParameters => 4;
    }
}
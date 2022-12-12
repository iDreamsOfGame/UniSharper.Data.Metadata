using UnityEngine;

namespace UniSharperEditor.Data.Metadata.EntityScriptPropertyStringEditors
{
    internal class UnityRangeIntPropertyStringEditor : UnityTypePropertyStringEditor<RangeInt>
    {
        protected override string ValuePropertyTypeName => PropertyTypeNames.Int32Array;
        
        protected override int NumberOfParameters => 2;
    }
}
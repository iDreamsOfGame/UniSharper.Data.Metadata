using System.Text;

namespace UniSharperEditor.Data.Metadata.EntityScriptPropertyStringEditors
{
    internal abstract class EntityScriptPropertyStringEditor : IEntityScriptPropertyStringEditor
    {
        public abstract void Edit(StringBuilder stringBuilder, EntityPropertyRawInfo rawInfo);
    }
}
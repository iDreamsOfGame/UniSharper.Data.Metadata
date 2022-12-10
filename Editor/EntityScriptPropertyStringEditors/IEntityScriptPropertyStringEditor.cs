using System.Text;

namespace UniSharperEditor.Data.Metadata.EntityScriptPropertyStringEditors
{
    internal interface IEntityScriptPropertyStringEditor
    {
        void Edit(StringBuilder stringBuilder, EntityPropertyRawInfo rawInfo);
    }
}
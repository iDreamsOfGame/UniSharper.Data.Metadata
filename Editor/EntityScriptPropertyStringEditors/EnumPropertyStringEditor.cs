using System.Text;
using UniSharper;

namespace UniSharperEditor.Data.Metadata.EntityScriptPropertyStringEditors
{
    internal class EnumPropertyStringEditor : EntityScriptPropertyStringEditor
    {
        public override void Edit(StringBuilder stringBuilder, EntityPropertyRawInfo rawInfo)
        {
            // Add enum int value property
            stringBuilder.AppendFormat(ScriptTemplate.ClassMemberFormatString.PropertyMember, rawInfo.Comment, "int", rawInfo.PropertyName);

            stringBuilder.Append(PlayerEnvironment.WindowsNewLine)
                .Append(PlayerEnvironment.WindowsNewLine);

            // Add enum property
            stringBuilder.AppendFormat(ScriptTemplate.ClassMemberFormatString.EnumProperty, rawInfo.Comment, rawInfo.Parameters[1], rawInfo.Parameters[0], rawInfo.PropertyName);
        }
    }
}
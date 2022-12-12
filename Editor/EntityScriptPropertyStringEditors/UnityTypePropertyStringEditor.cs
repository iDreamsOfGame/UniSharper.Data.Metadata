// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System.Text;
using UniSharper;

namespace UniSharperEditor.Data.Metadata.EntityScriptPropertyStringEditors
{
    internal abstract class UnityTypePropertyStringEditor<T> : EntityScriptPropertyStringEditor
    {
        protected abstract string ValuePropertyTypeName { get; }

        protected abstract int NumberOfParameters { get; }
        
        public override void Edit(StringBuilder stringBuilder, EntityPropertyRawInfo rawInfo)
        {
            // Add value property
            stringBuilder.AppendFormat(ScriptTemplate.ClassMemberFormatString.PropertyMember, rawInfo.Comment, ValuePropertyTypeName, rawInfo.PropertyName);
            
            stringBuilder.Append(PlayerEnvironment.WindowsNewLine)
                .Append(PlayerEnvironment.WindowsNewLine);
            
            // Add Unity type property
            var propertyType = typeof(T).FullName;
            var value = GenerateValueString(rawInfo.PropertyName);
            stringBuilder.AppendFormat(ScriptTemplate.ClassMemberFormatString.OnlyGetterPropertyMember, rawInfo.Comment, propertyType, rawInfo.Parameters[0], value);
        }

        private string GenerateValueString(string propertyName)
        {
            var valueStringBuilder = new StringBuilder("new(");
            for (var i = 0; i < NumberOfParameters; i++)
            {
                valueStringBuilder.AppendFormat($"{propertyName}[{i}]");
                if (i < NumberOfParameters - 1)
                    valueStringBuilder.AppendFormat(", ");
            }

            valueStringBuilder.AppendFormat(")");
            return valueStringBuilder.ToString();
        }
    }
}
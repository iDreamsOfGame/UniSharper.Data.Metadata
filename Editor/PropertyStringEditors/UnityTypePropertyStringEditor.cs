// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System.Text;
using UniSharper;

namespace UniSharperEditor.Data.Metadata.PropertyStringEditors
{
    internal class UnityTypePropertyStringEditor : PropertyStringEditor
    {
        public override void Edit(StringBuilder stringBuilder, EntityPropertyRawInfo rawInfo)
        {
            var valuePropertyTypeName = rawInfo.Parameters[0] as string;
            var valuePropertyName = rawInfo.PropertyName;
            var propertyType = rawInfo.PropertyType;
            var propertyName = rawInfo.Parameters[1] as string;
            var numberOfConstructorParameters = (int)rawInfo.Parameters[2];
            
            // Add value property
            stringBuilder.AppendFormat(ScriptTemplate.ClassMemberFormatString.AutoImplementedProperty, rawInfo.Comment, valuePropertyTypeName, valuePropertyName);
            
            stringBuilder.Append(PlayerEnvironment.WindowsNewLine)
                .Append(PlayerEnvironment.WindowsNewLine);
            
            // Add Unity type property
            var value = GenerateValueString(valuePropertyName, numberOfConstructorParameters);
            stringBuilder.AppendFormat(ScriptTemplate.ClassMemberFormatString.ExpressionBodiedGetterProperty, rawInfo.Comment, propertyType, propertyName, value);
        }

        private string GenerateValueString(string valuePropertyName, int numberOfConstructorParameters)
        {
            var valueStringBuilder = new StringBuilder("new(");
            for (var i = 0; i < numberOfConstructorParameters; i++)
            {
                valueStringBuilder.AppendFormat($"{valuePropertyName}[{i}]");
                if (i < numberOfConstructorParameters - 1)
                    valueStringBuilder.AppendFormat(", ");
            }

            valueStringBuilder.AppendFormat(")");
            return valueStringBuilder.ToString();
        }
    }
}
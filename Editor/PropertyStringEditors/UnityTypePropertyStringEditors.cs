// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System.Text;
using ReSharp.Extensions;
using UniSharper;

namespace UniSharperEditor.Data.Metadata.PropertyStringEditors
{
    internal class UnityTypeArrayPropertyStringEditor : UnityTypePropertyStringEditor
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
            
            // Add Unity type array property
            var value = GenerateValueString(propertyType, propertyName, valuePropertyName, numberOfConstructorParameters);
            stringBuilder.AppendFormat(ScriptTemplate.ClassMemberFormatString.OnlyGetterProperty, rawInfo.Comment, propertyType, propertyName, value);
        }
        
        private string GenerateValueString(string propertyType, string propertyName, string valuePropertyName, int numberOfConstructorParameters)
        {
            var valueStringBuilder = new StringBuilder();
            var fieldUnityType = propertyType.Replace("[]", string.Empty);
            var fieldName = propertyName.ToCamelCase();
            valueStringBuilder.Append($"if ({fieldName} != null)").Append(PlayerEnvironment.WindowsNewLine);
            valueStringBuilder.Append($"\t\t\t\t\treturn {fieldName};").Append(PlayerEnvironment.WindowsNewLine);
            valueStringBuilder.Append(PlayerEnvironment.WindowsNewLine);
            valueStringBuilder.Append($"\t\t\t\tvar list = new List<{fieldUnityType}>();").Append(PlayerEnvironment.WindowsNewLine);
            valueStringBuilder.Append($"\t\t\t\tfor (var i = 0; i < {valuePropertyName}.Length; i += {numberOfConstructorParameters})").Append(PlayerEnvironment.WindowsNewLine);
            valueStringBuilder.Append("\t\t\t\t{").Append(PlayerEnvironment.WindowsNewLine);

            for (var i = 0; i < numberOfConstructorParameters; i++)
            {
                var indexString = i == 0 ? "i" : $"i + {i}";
                valueStringBuilder.Append($"\t\t\t\t\tvar parameter{i + 1} = {valuePropertyName}[{indexString}];").Append(PlayerEnvironment.WindowsNewLine);
            }

            valueStringBuilder.Append($"\t\t\t\t\tlist.Add(new {fieldUnityType}(");

            for (var i = 0; i < numberOfConstructorParameters; i++)
            {
                if (i > 0)
                    valueStringBuilder.Append(", ");
                
                valueStringBuilder.Append($"parameter{i + 1}");
            }
            
            valueStringBuilder.Append("));").Append(PlayerEnvironment.WindowsNewLine);

            valueStringBuilder.Append("\t\t\t\t}").Append(PlayerEnvironment.WindowsNewLine);
            valueStringBuilder.Append($"\t\t\t\t{fieldName} = list.ToArray();").Append(PlayerEnvironment.WindowsNewLine);
            valueStringBuilder.Append($"\t\t\t\treturn {fieldName};");
            
            return valueStringBuilder.ToString();
        }
    }
    
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
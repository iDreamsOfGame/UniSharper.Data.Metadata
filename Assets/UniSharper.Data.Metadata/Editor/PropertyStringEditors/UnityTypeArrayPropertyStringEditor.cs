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

            // Add value property
            stringBuilder.AppendFormat(ScriptTemplate.ClassMemberFormatString.AutoImplementedProperty, rawInfo.Comment, valuePropertyTypeName, valuePropertyName);

            stringBuilder.Append(PlayerEnvironment.WindowsNewLine)
                .Append(PlayerEnvironment.WindowsNewLine);

            // Add Unity type array property
            var value = GenerateValueString(propertyType, propertyName, valuePropertyName);
            stringBuilder.AppendFormat(ScriptTemplate.ClassMemberFormatString.OnlyGetterProperty, rawInfo.Comment, propertyType, propertyName, value);
        }

        private static string GenerateValueString(string propertyType, string propertyName, string valuePropertyName)
        {
            var valueStringBuilder = new StringBuilder();
            var fieldUnityType = propertyType.Replace("[]", string.Empty);
            var fieldName = propertyName.ToCamelCase();
            valueStringBuilder.Append($"if ({fieldName}  == null)").Append(PlayerEnvironment.WindowsNewLine);
            valueStringBuilder.Append($"\t\t\t\t\t{fieldUnityType}Utility.TryParseArray({valuePropertyName}, out {fieldName});")
                .Append(PlayerEnvironment.WindowsNewLine)
                .Append(PlayerEnvironment.WindowsNewLine);
            valueStringBuilder.Append($"\t\t\t\treturn {fieldName};");
            return valueStringBuilder.ToString();
        }
    }
}
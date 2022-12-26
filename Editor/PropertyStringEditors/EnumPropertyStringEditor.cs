// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System.Text;
using UniSharper;

namespace UniSharperEditor.Data.Metadata.PropertyStringEditors
{
    internal class EnumPropertyStringEditor : PropertyStringEditor
    {
        public override void Edit(StringBuilder stringBuilder, EntityPropertyRawInfo rawInfo)
        {
            // Add enum int value property
            stringBuilder.AppendFormat(ScriptTemplate.ClassMemberFormatString.AutoImplementedProperty, rawInfo.Comment, PropertyTypeNames.Int32, rawInfo.PropertyName);

            stringBuilder.Append(PlayerEnvironment.WindowsNewLine)
                .Append(PlayerEnvironment.WindowsNewLine);

            // Add enum property
            stringBuilder.AppendFormat(ScriptTemplate.ClassMemberFormatString.EnumProperty, rawInfo.Comment, rawInfo.Parameters[1], rawInfo.Parameters[0], rawInfo.PropertyName);
        }
    }
}
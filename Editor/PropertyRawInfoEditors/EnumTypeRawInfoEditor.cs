// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Data;

namespace UniSharperEditor.Data.Metadata.EntityRawInfoEditors
{
    internal class EnumTypeRawInfoEditor : PropertyRawInfoEditor
    {
        internal EnumTypeRawInfoEditor(MetadataAssetSettings settings)
            : base(settings)
        {
        }
        
        public override EntityPropertyRawInfo Edit(DataTable dataTable, int column, string comment, string propertyType, string propertyName)
        {
            var parameters = new object[3];
            parameters[0] = propertyName;
            parameters[1] = $"{propertyName}Enum";

            var enumValues = new List<string> { "None" };
            var rows = dataTable.Rows;
            for (var row = Settings.EntityDataStartingRowIndex; row < rows.Count; row++)
            {
                var enumValue = rows[row][column].ToString().Trim();

                if (!string.IsNullOrEmpty(enumValue))
                {
                    enumValues.AddUnique(enumValue);
                }
            }

            parameters[2] = enumValues.ToArray();
            propertyName = $"{propertyName}Value";
            return new EntityPropertyRawInfo(comment, propertyType, propertyName, parameters);
        }
    }
}
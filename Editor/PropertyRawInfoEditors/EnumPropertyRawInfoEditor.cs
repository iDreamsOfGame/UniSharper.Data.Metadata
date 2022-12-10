using System.Collections.Generic;
using System.Data;

namespace UniSharperEditor.Data.Metadata.EntityRawInfoEditors
{
    internal class EnumPropertyRawInfoEditor : PropertyRawInfoEditor
    {
        internal EnumPropertyRawInfoEditor(MetadataAssetSettings settings, DataTable table)
            : base(settings, table)
        {
        }
        
        public override EntityPropertyRawInfo Edit(int column, string comment, string propertyType, string propertyName)
        {
            var parameters = new object[3];
            parameters[0] = propertyName;
            parameters[1] = $"{propertyName}Enum";

            var enumValues = new List<string> { "None" };
            var rows = Table.Rows;
            for (var row = Settings.EntityDataStartingRowIndex; row < rows.Count; row++)
            {
                var enumValue = rows[row][column].ToString().Trim();
            
                if (!string.IsNullOrEmpty(enumValue))
                {
                    enumValues.AddUnique(enumValue);
                }
            }

            parameters[2] = enumValues.ToArray();
            propertyName = $"{propertyName}IntValue";
            return new EntityPropertyRawInfo(comment, propertyType, propertyName, parameters);
        }
    }
}
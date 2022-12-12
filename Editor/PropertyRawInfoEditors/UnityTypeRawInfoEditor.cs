using System.Data;

namespace UniSharperEditor.Data.Metadata.EntityRawInfoEditors
{
    internal class UnityTypeRawInfoEditor : PropertyRawInfoEditor
    {
        public UnityTypeRawInfoEditor(MetadataAssetSettings settings, DataTable table)
            : base(settings, table)
        {
        }

        public override EntityPropertyRawInfo Edit(int column, string comment, string propertyType, string propertyName)
        {
            var parameters = new object[1];
            parameters[0] = propertyName;
            propertyName = $"{propertyName}Value";
            return new EntityPropertyRawInfo(comment, propertyType, propertyName, parameters);
        }
    }
}
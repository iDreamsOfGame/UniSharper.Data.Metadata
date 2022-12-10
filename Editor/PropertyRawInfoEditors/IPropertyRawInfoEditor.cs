namespace UniSharperEditor.Data.Metadata.EntityRawInfoEditors
{
    internal interface IPropertyRawInfoEditor
    {
        EntityPropertyRawInfo Edit(int column, string comment, string propertyType, string propertyName);
    }
}
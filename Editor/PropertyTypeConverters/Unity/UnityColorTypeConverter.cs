namespace UniSharperEditor.Data.Metadata.PropertyTypeConverters
{
    internal class UnityColorTypeConverter : PropertyTypeConverter
    {
        internal UnityColorTypeConverter(string propertyName)
            : base(propertyName)
        {
        }
        
        public override object Parse(char arrayElementSeparator, string value, params object[] parameters)
        {
            var color = ParseUnityColor(value);
            return new[] { color.r, color.g, color.b, color.a };
        }
    }
}
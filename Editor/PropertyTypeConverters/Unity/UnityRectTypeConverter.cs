namespace UniSharperEditor.Data.Metadata.PropertyTypeConverters
{
    internal class UnityRectTypeConverter : PropertyTypeConverter
    {
        internal UnityRectTypeConverter(string propertyName)
            : base(propertyName)
        {
        }
        
        public override object Parse(char arrayElementSeparator, string value, params object[] parameters)
        {
            var rect = ParseUnityRect(value);
            return new[] { rect.x, rect.y, rect.width, rect.height };
        }
    }
}
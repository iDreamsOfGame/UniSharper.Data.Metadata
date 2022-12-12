namespace UniSharperEditor.Data.Metadata.PropertyTypeConverters
{
    internal class UnityRectIntTypeConverter : PropertyTypeConverter
    {
        internal UnityRectIntTypeConverter(string propertyName)
            : base(propertyName)
        {
        }
        
        public override object Parse(char arrayElementSeparator, string value, params object[] parameters)
        {
            var rectInt = ParseUnityRectInt(value);
            return new[] { rectInt.x, rectInt.y, rectInt.width, rectInt.height };
        }
    }
}
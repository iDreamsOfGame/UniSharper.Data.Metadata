namespace UniSharperEditor.Data.Metadata.PropertyTypeConverters
{
    internal class UnityColor32TypeConverter : PropertyTypeConverter
    {
        internal UnityColor32TypeConverter(string propertyName)
            : base(propertyName)
        {
        }
        
        public override object Parse(char arrayElementSeparator, string value, params object[] parameters)
        {
            var color32 = ParseUnityColor32(value);
            return new[] { color32.r, color32.g, color32.b, color32.a };
        }
    }
}
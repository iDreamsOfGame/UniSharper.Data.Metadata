namespace UniSharperEditor.Data.Metadata.PropertyTypeConverters
{
    internal class UnityVector4TypeConverter : PropertyTypeConverter
    {
        internal UnityVector4TypeConverter(string propertyName)
            : base(propertyName)
        {
        }
        
        public override object Parse(char arrayElementSeparator, string value, params object[] parameters)
        {
            var vector4 = ParseUnityVector4(value);
            return new[] { vector4.x, vector4.y, vector4.z, vector4.w };
        }
    }
}
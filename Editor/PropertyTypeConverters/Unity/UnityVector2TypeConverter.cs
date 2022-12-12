namespace UniSharperEditor.Data.Metadata.PropertyTypeConverters
{
    internal class UnityVector2TypeConverter : PropertyTypeConverter
    {
        internal UnityVector2TypeConverter(string propertyName)
            : base(propertyName)
        {
        }

        public override object Parse(char arrayElementSeparator, string value, params object[] parameters)
        {
            var vector2 = ParseUnityVector2(value);
            return new[] { vector2.x, vector2.y };
        }
    }
}
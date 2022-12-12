namespace UniSharperEditor.Data.Metadata.PropertyTypeConverters
{
    internal class UnityVector3TypeConverter : PropertyTypeConverter
    {
        internal UnityVector3TypeConverter(string propertyName)
            : base(propertyName)
        {
        }
        
        public override object Parse(char arrayElementSeparator, string value, params object[] parameters)
        {
            var vector3 = ParseUnityVector3(value);
            return new[] { vector3.x, vector3.y, vector3.z };
        }
    }
}
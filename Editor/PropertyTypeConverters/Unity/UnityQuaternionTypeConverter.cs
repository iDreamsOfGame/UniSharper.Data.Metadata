namespace UniSharperEditor.Data.Metadata.PropertyTypeConverters
{
    internal class UnityQuaternionTypeConverter : PropertyTypeConverter
    {
        internal UnityQuaternionTypeConverter(string propertyName)
            : base(propertyName)
        {
        }
        
        public override object Parse(char arrayElementSeparator, string value, params object[] parameters)
        {
            var quaternion = ParseUnityQuaternion(value);
            return new[] { quaternion.x, quaternion.y, quaternion.z, quaternion.w };
        }
    }
}
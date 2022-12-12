namespace UniSharperEditor.Data.Metadata.PropertyTypeConverters
{
    internal class UnityVector3IntTypeConverter : PropertyTypeConverter
    {
        internal UnityVector3IntTypeConverter(string propertyName)
            : base(propertyName)
        {
        }
        
        public override object Parse(char arrayElementSeparator, string value, params object[] parameters)
        {
            var vector3Int = ParseUnityVector3Int(value);
            return new[] { vector3Int.x, vector3Int.y, vector3Int.z };
        }
    }
}
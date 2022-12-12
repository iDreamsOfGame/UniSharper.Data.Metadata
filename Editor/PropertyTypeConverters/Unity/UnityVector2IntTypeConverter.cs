namespace UniSharperEditor.Data.Metadata.PropertyTypeConverters
{
    internal class UnityVector2IntTypeConverter : PropertyTypeConverter
    {
        internal UnityVector2IntTypeConverter(string propertyName)
            : base(propertyName)
        {
        }
        
        public override object Parse(char arrayElementSeparator, string value, params object[] parameters)
        {
            var vector2Int = ParseUnityVector2Int(value);
            return new[] { vector2Int.x, vector2Int.y };
        }
    }
}
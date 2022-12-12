namespace UniSharperEditor.Data.Metadata.PropertyTypeConverters
{
    internal class UnityRangeIntTypeConverter : PropertyTypeConverter
    {
        internal UnityRangeIntTypeConverter(string propertyName)
            : base(propertyName)
        {
        }
        
        public override object Parse(char arrayElementSeparator, string value, params object[] parameters)
        {
            var rangeInt = ParseUnityRangeInt(value);
            return new[] { rangeInt.start, rangeInt.length };
        }
    }
}
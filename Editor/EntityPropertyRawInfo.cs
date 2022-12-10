namespace UniSharperEditor.Data.Metadata
{
    /// <summary>
    /// The raw information struct of metadata entity property.
    /// </summary>
    internal readonly struct EntityPropertyRawInfo
    {
        public EntityPropertyRawInfo(string comment, string propertyType, string propertyName, params object[] parameters)
        {
            Comment = comment;
            PropertyType = propertyType;
            PropertyName = propertyName;
            Parameters = parameters;
        }

        /// <summary>
        /// The comment content of entity property.
        /// </summary>
        public string Comment { get; }
        
        /// <summary>
        /// The type string of entity property.
        /// </summary>
        public string PropertyType { get; }

        /// <summary>
        /// The name of entity property.
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// The parameters.
        /// </summary>
        public object[] Parameters { get; }
    }
}
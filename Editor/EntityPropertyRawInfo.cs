// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

namespace UniSharperEditor.Data.Metadata
{
    /// <summary>
    /// The raw information struct of metadata entity property.
    /// </summary>
    internal readonly struct EntityPropertyRawInfo
    {
        public EntityPropertyRawInfo(int column, string comment, string propertyType, string propertyName, params object[] parameters)
        {
            Column = column;
            Comment = comment;
            PropertyType = propertyType;
            PropertyName = propertyName;
            Parameters = parameters;
        }

        /// <summary>
        /// The index of column in excel data table.
        /// </summary>
        public int Column { get; }

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
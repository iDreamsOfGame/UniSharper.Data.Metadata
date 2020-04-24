// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

namespace UniSharper.Data.Metadata.Parsers
{
    /// <summary>
    /// The interface of property type converter.
    /// </summary>
    internal interface IPropertyTypeConverter
    {
        #region Methods

        /// <summary>
        /// Parses the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The parsed data.</returns>
        object Parse(string value, params object[] parameters);

        #endregion Methods
    }
}
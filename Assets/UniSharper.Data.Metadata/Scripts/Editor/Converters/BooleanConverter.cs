// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License. See LICENSE in the
// project root for license information.

namespace UniSharperEditor.Data.Metadata.Converters
{
    internal class BooleanConverter : PropertyTypeConverter
    {
        #region Constructors

        internal BooleanConverter(string propertyName)
            : base(propertyName)
        {
        }

        #endregion Constructors

        #region Methods

        public override object Parse(string value, params object[] parameters)
        {
            return ParseBoolean(value);
        }

        #endregion Methods
    }
}
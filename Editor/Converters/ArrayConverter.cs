// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System;

namespace UniSharperEditor.Data.Metadata.Converters
{
    internal class ArrayConverter : PropertyTypeConverter
    {
        #region Fields

        private static readonly char[] ArrayElementSeparator = new char[] { '|' };

        #endregion Fields

        #region Constructors

        internal ArrayConverter(string propertyName)
            : base(propertyName)
        {
        }

        #endregion Constructors

        #region Methods

        public override object Parse(string value, params object[] parameters) 
            => string.IsNullOrEmpty(value) ? new string[] { } : value.Split(ArrayElementSeparator, StringSplitOptions.RemoveEmptyEntries);

        #endregion Methods
    }
}
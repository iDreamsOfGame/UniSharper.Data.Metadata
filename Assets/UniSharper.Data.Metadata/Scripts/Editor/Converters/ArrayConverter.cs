// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License. See LICENSE in the
// project root for license information.

using System;

namespace UniSharperEditor.Data.Metadata.Converters
{
    internal class ArrayConverter : PropertyTypeConverter
    {
        #region Fields

        private const string ArrayElementSeparator = "|";

        #endregion Fields

        #region Constructors

        internal ArrayConverter(string propertyName)
            : base(propertyName)
        {
        }

        #endregion Constructors

        #region Methods

        public override object Parse(string value, params object[] parameters) => value.Split(ArrayElementSeparator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

        #endregion Methods
    }
}
// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License. See LICENSE in the
// project root for license information.

namespace UniSharperEditor.Data.Metadata.Converters
{
    internal class NumberArrayConverter<T> : ArrayConverter
    {
        #region Constructors

        internal NumberArrayConverter(string propertyName)
            : base(propertyName)
        {
        }

        #endregion Constructors

        #region Methods

        public override object Parse(string value, params object[] parameters)
        {
            if (!(base.Parse(value) is string[] array))
                return null;

            var result = new T[array.Length];
            for (var i = 0; i < result.Length; i++)
            {
                result[i] = ParseNumber<T>(array[i]);
            }
            return result;
        }

        #endregion Methods
    }
}
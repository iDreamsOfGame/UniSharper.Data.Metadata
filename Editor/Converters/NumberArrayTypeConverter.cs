// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

namespace UniSharperEditor.Data.Metadata.Converters
{
    internal class NumberArrayTypeConverter<T> : ArrayTypeConverter
    {
        internal NumberArrayTypeConverter(string propertyName)
            : base(propertyName)
        {
        }

        public override object Parse(char arrayElementSeparator, string value, params object[] parameters)
        {
            if (base.Parse(arrayElementSeparator, value) is not string[] array)
                return null;

            var result = new T[array.Length];
            for (var i = 0; i < result.Length; i++)
            {
                result[i] = ParseNumber<T>(array[i]);
            }
            return result;
        }
    }
}
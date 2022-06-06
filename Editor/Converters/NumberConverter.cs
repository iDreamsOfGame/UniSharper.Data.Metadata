// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

namespace UniSharperEditor.Data.Metadata.Converters
{
    internal class NumberConverter<T> : PropertyTypeConverter
    {
        internal NumberConverter(string propertyName)
            : base(propertyName)
        {
        }

        public override object Parse(string value, params object[] parameters) => ParseNumber<T>(value);
    }
}
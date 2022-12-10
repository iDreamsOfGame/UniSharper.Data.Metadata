// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

namespace UniSharperEditor.Data.Metadata.Converters
{
    internal class BooleanTypeConverter : PropertyTypeConverter
    {
        internal BooleanTypeConverter(string propertyName)
            : base(propertyName)
        {
        }

        public override object Parse(char arrayElementSeparator, string value, params object[] parameters) => ParseBoolean(value);
    }
}
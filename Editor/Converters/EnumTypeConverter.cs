// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System;

namespace UniSharperEditor.Data.Metadata.Converters
{
    internal class EnumTypeConverter : PropertyTypeConverter
    {
        internal EnumTypeConverter(string propertyName)
            : base(propertyName)
        {
        }

        public override object Parse(char arrayElementSeparator, string value, params object[] parameters)
        {
            if (string.IsNullOrEmpty(value)) 
                return 0;
            
            var enumValues = parameters[2] as string[];
            return Array.FindIndex(enumValues ?? Array.Empty<string>(), enumValue => enumValue.Equals(value));
        }
    }
}
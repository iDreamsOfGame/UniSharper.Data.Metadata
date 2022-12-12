// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System;

namespace UniSharperEditor.Data.Metadata.PropertyTypeConverters
{
    internal class UnityColor32TypeConverter : PropertyTypeConverter
    {
        internal UnityColor32TypeConverter(string propertyName)
            : base(propertyName)
        {
        }

        public override object Parse(char arrayElementSeparator, string value, params object[] parameters)
        {
            var color32 = value.ToColor32();
            return new[] { color32.r, color32.g, color32.b, color32.a };
        }
    }
}
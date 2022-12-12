// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System;

namespace UniSharperEditor.Data.Metadata.PropertyTypeConverters
{
    internal class UnityColorTypeConverter : PropertyTypeConverter
    {
        internal UnityColorTypeConverter(string propertyName)
            : base(propertyName)
        {
        }
        
        public override object Parse(char arrayElementSeparator, string value, params object[] parameters)
        {
            var color = value.ToColor();
            return new[] { color.r, color.g, color.b, color.a };
        }
    }
}
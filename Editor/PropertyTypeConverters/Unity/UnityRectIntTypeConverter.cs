// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System;

namespace UniSharperEditor.Data.Metadata.PropertyTypeConverters
{
    internal class UnityRectIntTypeConverter : PropertyTypeConverter
    {
        internal UnityRectIntTypeConverter(string propertyName)
            : base(propertyName)
        {
        }
        
        public override object Parse(char arrayElementSeparator, string value, params object[] parameters)
        {
            var rectInt = value.ToRectInt();
            return new[] { rectInt.x, rectInt.y, rectInt.width, rectInt.height };
        }
    }
}
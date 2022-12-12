// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System;

namespace UniSharperEditor.Data.Metadata.PropertyTypeConverters
{
    internal class UnityRangeIntTypeConverter : PropertyTypeConverter
    {
        internal UnityRangeIntTypeConverter(string propertyName)
            : base(propertyName)
        {
        }
        
        public override object Parse(char arrayElementSeparator, string value, params object[] parameters)
        {
            var rangeInt = value.ToRangeInt();
            return new[] { rangeInt.start, rangeInt.length };
        }
    }
}
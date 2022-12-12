// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System;

namespace UniSharperEditor.Data.Metadata.PropertyTypeConverters
{
    internal class UnityVector2IntTypeConverter : PropertyTypeConverter
    {
        internal UnityVector2IntTypeConverter(string propertyName)
            : base(propertyName)
        {
        }
        
        public override object Parse(char arrayElementSeparator, string value, params object[] parameters)
        {
            var vector2Int = value.ToVector2Int();
            return new[] { vector2Int.x, vector2Int.y };
        }
    }
}
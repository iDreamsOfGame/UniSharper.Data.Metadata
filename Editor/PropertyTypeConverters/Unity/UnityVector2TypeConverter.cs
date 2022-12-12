// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System;

namespace UniSharperEditor.Data.Metadata.PropertyTypeConverters
{
    internal class UnityVector2TypeConverter : PropertyTypeConverter
    {
        public override object Parse(char arrayElementSeparator, string value, params object[] parameters)
        {
            var vector2 = value.ToVector2();
            return new[] { vector2.x, vector2.y };
        }
    }
}
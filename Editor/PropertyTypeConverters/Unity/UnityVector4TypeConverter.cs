// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System;

namespace UniSharperEditor.Data.Metadata.PropertyTypeConverters
{
    internal class UnityVector4TypeConverter : PropertyTypeConverter
    {
        public override object Parse(char arrayElementSeparator, string value, params object[] parameters)
        {
            var vector4 = value.ToVector4();
            return new[] { vector4.x, vector4.y, vector4.z, vector4.w };
        }
    }
}
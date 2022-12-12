// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System;

namespace UniSharperEditor.Data.Metadata.PropertyTypeConverters
{
    internal class UnityQuaternionTypeConverter : PropertyTypeConverter
    {
        internal UnityQuaternionTypeConverter(string propertyName)
            : base(propertyName)
        {
        }
        
        public override object Parse(char arrayElementSeparator, string value, params object[] parameters)
        {
            var quaternion = value.ToQuaternion();
            return new[] { quaternion.x, quaternion.y, quaternion.z, quaternion.w };
        }
    }
}
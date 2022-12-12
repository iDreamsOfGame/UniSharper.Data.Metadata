// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System;

namespace UniSharperEditor.Data.Metadata.PropertyTypeConverters
{
    internal class UnityVector3IntTypeConverter : PropertyTypeConverter
    {
        internal UnityVector3IntTypeConverter(string propertyName)
            : base(propertyName)
        {
        }
        
        public override object Parse(char arrayElementSeparator, string value, params object[] parameters)
        {
            var vector3Int = value.ToVector3Int();
            return new[] { vector3Int.x, vector3Int.y, vector3Int.z };
        }
    }
}
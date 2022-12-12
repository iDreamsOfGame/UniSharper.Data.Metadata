// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System;

namespace UniSharperEditor.Data.Metadata.PropertyTypeConverters
{
    internal class ArrayTypeConverter : PropertyTypeConverter
    {
        public override object Parse(char arrayElementSeparator, string value, params object[] parameters) => 
            string.IsNullOrEmpty(value) ? new string[] { } : value.Split(arrayElementSeparator, StringSplitOptions.RemoveEmptyEntries);
    }
}
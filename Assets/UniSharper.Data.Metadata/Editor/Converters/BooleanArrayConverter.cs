// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

namespace UniSharperEditor.Data.Metadata.Converters
{
    internal class BooleanArrayConverter : ArrayConverter
    {
        internal BooleanArrayConverter(string propertyName)
            : base(propertyName)
        {
        }

        public override object Parse(string value, params object[] parameters)
        {
            if (base.Parse(value) is not string[] array)
                return null;

            var result = new bool[array.Length];
            for (var i = 0; i < result.Length; i++)
            {
                result[i] = ParseBoolean(array[i]);
            }
            return result;
        }
    }
}
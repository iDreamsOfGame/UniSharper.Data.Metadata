// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System;
using System.Globalization;

namespace UniSharperEditor.Data.Metadata.PropertyTypeConverters
{
    internal class PropertyTypeConverter : IPropertyTypeConverter
    {
        internal PropertyTypeConverter(string propertyName)
        {
            PropertyName = propertyName;
        }

        protected string PropertyName { get; }

        public virtual object Parse(char arrayElementSeparator, string value, params object[] parameters)
        {
            if (string.IsNullOrEmpty(value)) 
                return value;
            
            value = value.Trim();
            return value;
        }

        protected static bool ParseBoolean(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            var textInfo = CultureInfo.InvariantCulture.TextInfo;
            value = textInfo.ToTitleCase(value.Trim());
            bool.TryParse(value, out var result);
            return result;
        }

        protected static T ParseNumber<T>(string value)
        {
            var result = default(T);
            var targetType = typeof(T);
            value = value.Trim();
            var args = new object[] { value, NumberStyles.Any, CultureInfo.InvariantCulture, result };

            if (!string.IsNullOrEmpty(value))
            {
                targetType.InvokeStaticMethod("TryParse", new[]
                {
                    typeof(string), typeof(NumberStyles), typeof(CultureInfo), targetType.MakeByRefType()
                }, ref args);
            }

            return (T)args[3];
        }
    }
}
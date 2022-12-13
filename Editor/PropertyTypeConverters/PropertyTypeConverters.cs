// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System;
using System.Globalization;
using ReSharp.Extensions;

namespace UniSharperEditor.Data.Metadata.PropertyTypeConverters
{
    internal class EnumTypeConverter : PropertyTypeConverter
    {
        public override object Parse(char arrayElementSeparator, string value, params object[] parameters)
        {
            if (string.IsNullOrEmpty(value)) 
                return 0;
            
            var enumValues = parameters[2] as string[];
            return Array.FindIndex(enumValues ?? Array.Empty<string>(), enumValue => enumValue.Equals(value));
        }
    }
    
    internal class BooleanTypeConverter : PropertyTypeConverter
    {
        public override object Parse(char arrayElementSeparator, string value, params object[] parameters) => ParseBoolean(value);
    }
    
    internal class NumberTypeConverter<T> : PropertyTypeConverter
    {
        public override object Parse(char arrayElementSeparator, string value, params object[] parameters) => ParseNumber<T>(value);
    }
    
    internal class PropertyTypeConverter : IPropertyTypeConverter
    {
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
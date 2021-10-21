// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System;
using System.Globalization;
using UniSharper.Data.Metadata.Parsers;

namespace UniSharperEditor.Data.Metadata.Converters
{
    internal class PropertyTypeConverter : IPropertyTypeConverter
    {
        #region Constructors

        internal PropertyTypeConverter(string propertyName)
        {
            PropertyName = propertyName;
        }

        #endregion Constructors

        #region Properties

        protected string PropertyName { get; }

        #endregion Properties

        #region Methods

        public virtual object Parse(string value, params object[] parameters)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

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

        #endregion Methods
    }
}
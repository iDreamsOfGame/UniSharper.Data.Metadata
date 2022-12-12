// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System;
using System.Globalization;
using UnityEngine;

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
            if (!string.IsNullOrEmpty(value))
            {
                value = value.Trim();
                return value;
            }

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
        
        protected static Vector2 ParseUnityVector2(string value) => value.ToVector2();

        protected static Vector2Int ParseUnityVector2Int(string value) => value.ToVector2Int();
        
        protected static Vector3 ParseUnityVector3(string value) => value.ToVector3();
        
        protected static Vector3Int ParseUnityVector3Int(string value) => value.ToVector3Int();
        
        protected static Vector4 ParseUnityVector4(string value) => value.ToVector4();

        protected static RangeInt ParseUnityRangeInt(string value) => value.ToRangeInt();

        protected static Quaternion ParseUnityQuaternion(string value) => value.ToQuaternion();

        protected static Rect ParseUnityRect(string value) => value.ToRect();

        protected static RectInt ParseUnityRectInt(string value) => value.ToRectInt();

        protected static Color ParseUnityColor(string value) => value.ToColor();

        protected static Color32 ParseUnityColor32(string value) => value.ToColor32();
    }
}
// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System;
using System.Globalization;
using UnityEngine;

namespace UniSharperEditor.Data.Metadata.Converters
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
        
        protected static Vector2 ParseUnityEngineVector2(string value) => value.ToVector2();

        protected static Vector2Int ParseUnityEngineVector2Int(string value) => value.ToVector2Int();
        
        protected static Vector3 ParseUnityEngineVector3(string value) => value.ToVector3();
        
        protected static Vector3Int ParseUnityEngineVector3Int(string value) => value.ToVector3Int();
        
        protected static Vector4 ParseUnityEngineVector4(string value) => value.ToVector4();

        protected static RangeInt ParseUnityEngineRangeInt(string value) => value.ToRangeInt();

        protected static Quaternion ParseUnityEngineQuaternion(string value) => value.ToQuaternion();

        protected static Rect ParseUnityEngineRect(string value) => value.ToRect();

        protected static RectInt ParseUnityEngineRectInt(string value) => value.ToRectInt();

        protected static Color ParseUnityEngineColor(string value) => value.ToColor();

        protected static Color32 ParseUnityEngineColor32(string value) => value.ToColor32();
    }
}
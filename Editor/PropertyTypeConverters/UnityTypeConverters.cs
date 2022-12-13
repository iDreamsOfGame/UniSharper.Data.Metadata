// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using UniSharper.Extensions;

namespace UniSharperEditor.Data.Metadata.PropertyTypeConverters
{
    internal class UnityVector2TypeConverter : PropertyTypeConverter
    {
        public override object Parse(char arrayElementSeparator, string value, params object[] parameters)
        {
            Vector2Utility.TryParse(value, out var result);
            return new[] { result.x, result.y };
        }
    }
    
    internal class UnityVector2IntTypeConverter : PropertyTypeConverter
    {
        public override object Parse(char arrayElementSeparator, string value, params object[] parameters)
        {
            Vector2IntUtility.TryParse(value, out var result);
            return new[] { result.x, result.y };
        }
    }
    
    internal class UnityVector3TypeConverter : PropertyTypeConverter
    {
        public override object Parse(char arrayElementSeparator, string value, params object[] parameters)
        {
            Vector3Utility.TryParse(value, out var result);
            return new[] { result.x, result.y, result.z };
        }
    }
    
    internal class UnityVector3IntTypeConverter : PropertyTypeConverter
    {
        public override object Parse(char arrayElementSeparator, string value, params object[] parameters)
        {
            Vector3IntUtility.TryParse(value, out var result);
            return new[] { result.x, result.y, result.z };
        }
    }
    
    internal class UnityVector4TypeConverter : PropertyTypeConverter
    {
        public override object Parse(char arrayElementSeparator, string value, params object[] parameters)
        {
            Vector4Utility.TryParse(value, out var result);
            return new[] { result.x, result.y, result.z, result.w };
        }
    }
    
    internal class UnityRectTypeConverter : PropertyTypeConverter
    {
        public override object Parse(char arrayElementSeparator, string value, params object[] parameters)
        {
            RectUtility.TryParse(value, out var result);
            return new[] { result.x, result.y, result.width, result.height };
        }
    }
    
    internal class UnityRectIntTypeConverter : PropertyTypeConverter
    {
        public override object Parse(char arrayElementSeparator, string value, params object[] parameters)
        {
            RectIntUtility.TryParse(value, out var result);
            return new[] { result.x, result.y, result.width, result.height };
        }
    }
    
    internal class UnityRangeIntTypeConverter : PropertyTypeConverter
    {
        public override object Parse(char arrayElementSeparator, string value, params object[] parameters)
        {
            RangeIntUtility.TryParse(value, out var result);
            return new[] { result.start, result.length };
        }
    }
    
    internal class UnityQuaternionTypeConverter : PropertyTypeConverter
    {
        public override object Parse(char arrayElementSeparator, string value, params object[] parameters)
        {
            QuaternionUtility.TryParse(value, out var result);
            return new[] { result.x, result.y, result.z, result.w };
        }
    }
    
    internal class UnityColorTypeConverter : PropertyTypeConverter
    {
        public override object Parse(char arrayElementSeparator, string value, params object[] parameters)
        {
            ColorUtility.TryParse(value, out var result);
            return new[] { result.r, result.g, result.b, result.a };
        }
    }
    
    internal class UnityColor32TypeConverter : PropertyTypeConverter
    {
        public override object Parse(char arrayElementSeparator, string value, params object[] parameters)
        {
            Color32Utility.TryParse(value, out var result);
            return new[] { result.r, result.g, result.b, result.a };
        }
    }
}
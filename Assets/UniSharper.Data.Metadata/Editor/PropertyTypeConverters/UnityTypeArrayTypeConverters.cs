// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System.Collections.Generic;
using UniSharper.Extensions;

namespace UniSharperEditor.Data.Metadata.PropertyTypeConverters
{
    internal class UnityVector2ArrayTypeConverter : ArrayTypeConverter
    {
        public override object Parse(char arrayElementSeparator, string value, params object[] parameters)
        {
            var elements = Vector2Utility.ParseArray(value, new string(new [] { arrayElementSeparator }));
            var values = new List<float>();
            foreach (var element in elements)
            {
                values.Add(element.x);
                values.Add(element.y);
            }
            return values.ToArray();
        }
    }
    
    internal class UnityVector2IntArrayTypeConverter : ArrayTypeConverter
    {
        public override object Parse(char arrayElementSeparator, string value, params object[] parameters)
        {
            var elements = Vector2IntUtility.ParseArray(value, new string(new [] { arrayElementSeparator }));
            var values = new List<int>();
            foreach (var element in elements)
            {
                values.Add(element.x);
                values.Add(element.y);
            }
            return values.ToArray();
        }
    }
    
    internal class UnityVector3ArrayTypeConverter : ArrayTypeConverter
    {
        public override object Parse(char arrayElementSeparator, string value, params object[] parameters)
        {
            var elements = Vector3Utility.ParseArray(value, new string(new [] { arrayElementSeparator }));
            var values = new List<float>();
            foreach (var element in elements)
            {
                values.Add(element.x);
                values.Add(element.y);
                values.Add(element.z);
            }
            return values.ToArray();
        }
    }
    
    internal class UnityVector3IntArrayTypeConverter : ArrayTypeConverter
    {
        public override object Parse(char arrayElementSeparator, string value, params object[] parameters)
        {
            var elements = Vector3IntUtility.ParseArray(value, new string(new [] { arrayElementSeparator }));
            var values = new List<int>();
            foreach (var element in elements)
            {
                values.Add(element.x);
                values.Add(element.y);
                values.Add(element.z);
            }
            return values.ToArray();
        }
    }
    
    internal class UnityVector4ArrayTypeConverter : ArrayTypeConverter
    {
        public override object Parse(char arrayElementSeparator, string value, params object[] parameters)
        {
            var elements = Vector4Utility.ParseArray(value, new string(new [] { arrayElementSeparator }));
            var values = new List<float>();
            foreach (var element in elements)
            {
                values.Add(element.x);
                values.Add(element.y);
                values.Add(element.z);
                values.Add(element.w);
            }
            return values.ToArray();
        }
    }
    
    internal class UnityRangeIntArrayTypeConverter : ArrayTypeConverter
    {
        public override object Parse(char arrayElementSeparator, string value, params object[] parameters)
        {
            var elements = RangeIntUtility.ParseArray(value, new string(new [] { arrayElementSeparator }));
            var values = new List<int>();
            foreach (var element in elements)
            {
                values.Add(element.start);
                values.Add(element.length);
            }
            return values.ToArray();
        }
    }
    
    internal class UnityQuaternionArrayTypeConverter : ArrayTypeConverter
    {
        public override object Parse(char arrayElementSeparator, string value, params object[] parameters)
        {
            var elements = QuaternionUtility.ParseArray(value, new string(new [] { arrayElementSeparator }));
            var values = new List<float>();
            foreach (var element in elements)
            {
                values.Add(element.x);
                values.Add(element.y);
                values.Add(element.z);
                values.Add(element.w);
            }
            return values.ToArray();
        }
    }
    
    internal class UnityRectArrayTypeConverter : ArrayTypeConverter
    {
        public override object Parse(char arrayElementSeparator, string value, params object[] parameters)
        {
            var elements = RectUtility.ParseArray(value, new string(new [] { arrayElementSeparator }));
            var values = new List<float>();
            foreach (var element in elements)
            {
                values.Add(element.x);
                values.Add(element.y);
                values.Add(element.width);
                values.Add(element.height);
            }
            return values.ToArray();
        }
    }
    
    internal class UnityRectIntArrayTypeConverter : ArrayTypeConverter
    {
        public override object Parse(char arrayElementSeparator, string value, params object[] parameters)
        {
            var elements = RectIntUtility.ParseArray(value, new string(new [] { arrayElementSeparator }));
            var values = new List<int>();
            foreach (var element in elements)
            {
                values.Add(element.x);
                values.Add(element.y);
                values.Add(element.width);
                values.Add(element.height);
            }
            return values.ToArray();
        }
    }
    
    internal class UnityColorArrayTypeConverter : ArrayTypeConverter
    {
        public override object Parse(char arrayElementSeparator, string value, params object[] parameters)
        {
            var elements = ColorUtility.ParseArray(value, new string(new [] { arrayElementSeparator }));
            var values = new List<float>();
            foreach (var element in elements)
            {
                values.Add(element.r);
                values.Add(element.g);
                values.Add(element.b);
                values.Add(element.a);
            }
            return values.ToArray();
        }
    }
    
    internal class UnityColor32ArrayTypeConverter : ArrayTypeConverter
    {
        public override object Parse(char arrayElementSeparator, string value, params object[] parameters)
        {
            var elements = Color32Utility.ParseArray(value, new string(new [] { arrayElementSeparator }));
            var values = new List<byte>();
            foreach (var element in elements)
            {
                values.Add(element.r);
                values.Add(element.g);
                values.Add(element.b);
                values.Add(element.a);
            }
            return values.ToArray();
        }
    }
}
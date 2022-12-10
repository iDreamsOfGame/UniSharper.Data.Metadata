using System;
using System.Reflection;
using UniSharperEditor.Data.Metadata.Converters;

namespace UniSharperEditor.Data.Metadata
{
    internal class UnityEnginePropertyTypeConverter<T> : PropertyTypeConverter where T : struct
    {
        internal UnityEnginePropertyTypeConverter(string propertyName)
            : base(propertyName)
        {
        }

        public override object Parse(char arrayElementSeparator, string value, params object[] parameters)
        {
            var methodName = $"ParseUnityEngine{typeof(T).Name}";
            return typeof(PropertyTypeConverter).InvokeStaticMethod(methodName, new[] { typeof(string) }, new object[] { value }, 
                BindingFlags.Static | BindingFlags.NonPublic);
        }
    }
}
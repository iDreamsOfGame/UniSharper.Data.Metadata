// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using UniSharper.Data.Metadata.Parsers;

namespace UniSharperEditor.Data.Metadata.Converters
{
    /// <summary>
    /// The factory class of property type converter.
    /// </summary>
    internal static class PropertyTypeConverterFactory
    {
        private static readonly Dictionary<Type, IPropertyTypeConverter> ConvertersCache = new();

        /// <summary>
        /// The map of Type parsers.
        /// </summary>
        private static readonly Dictionary<string, Type> ConvertersMap = new()
        {
            { "string", typeof(PropertyTypeConverter) },
            { "bool", typeof(BooleanConverter) },
            { "byte", typeof(NumberConverter<byte>) },
            { "sbyte", typeof(NumberConverter<sbyte>) },
            { "decimal", typeof(NumberConverter<decimal>) },
            { "double", typeof(NumberConverter<double>) },
            { "float", typeof(NumberConverter<float>) },
            { "int", typeof(NumberConverter<int>) },
            { "uint", typeof(NumberConverter<uint>) },
            { "long", typeof(NumberConverter<long>) },
            { "ulong", typeof(NumberConverter<ulong>) },
            { "short", typeof(NumberConverter<short>) },
            { "ushort", typeof(NumberConverter<ushort>) },
            { "enum", typeof(EnumConverter) },
            { "string[]", typeof(ArrayConverter) },
            { "bool[]", typeof(BooleanArrayConverter) },
            { "byte[]", typeof(NumberArrayConverter<byte>) },
            { "sbyte[]", typeof(NumberArrayConverter<sbyte>) },
            { "decimal[]", typeof(NumberArrayConverter<decimal>) },
            { "double[]", typeof(NumberArrayConverter<double>) },
            { "float[]", typeof(NumberArrayConverter<float>) },
            { "int[]", typeof(NumberArrayConverter<int>) },
            { "uint[]", typeof(NumberArrayConverter<uint>) },
            { "long[]", typeof(NumberArrayConverter<long>) },
            { "ulong[]", typeof(NumberArrayConverter<ulong>) },
            { "short[]", typeof(NumberArrayConverter<short>) },
            { "ushort[]", typeof(NumberArrayConverter<ushort>) }
        };

        internal static IPropertyTypeConverter GetTypeConverter(string typeString, string propertyName)
        {
            if (string.IsNullOrEmpty(typeString))
                return null;

            var type = GetConverterType(typeString);

            if (type == null)
                return null;

            IPropertyTypeConverter typeParser;

            if (ConvertersCache.ContainsKey(type))
            {
                typeParser = ConvertersCache[type];
            }
            else
            {
                typeParser = CreateTypeConverter(typeString, propertyName);
                ConvertersCache.Add(type, typeParser);
            }

            return typeParser;
        }

        private static IPropertyTypeConverter CreateTypeConverter(string typeString, string propertyName)
        {
            var type = GetConverterType(typeString);
            return (IPropertyTypeConverter)type?.InvokeConstructor(new object[] { propertyName });
        }

        private static Type GetConverterType(string typeString) => 
            !string.IsNullOrEmpty(typeString) && ConvertersMap.ContainsKey(typeString) ? ConvertersMap[typeString] : null;
    }
}
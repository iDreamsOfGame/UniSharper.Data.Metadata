// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using UniSharperEditor.Data.Metadata.Converters;

namespace UniSharperEditor.Data.Metadata
{
    internal static class PropertyTypeConverterFactory
    {
        private static readonly Dictionary<Type, IPropertyTypeConverter> ConvertersCache = new();
        
        private static readonly Dictionary<string, Type> PropertyTypeConverterTypeMap = new()
        {
            { PropertyTypeNames.String, typeof(PropertyTypeConverter) },
            { PropertyTypeNames.Boolean, typeof(BooleanTypeConverter) },
            { PropertyTypeNames.Byte, typeof(NumberTypeConverter<byte>) },
            { PropertyTypeNames.SByte, typeof(NumberTypeConverter<sbyte>) },
            { PropertyTypeNames.Decimal, typeof(NumberTypeConverter<decimal>) },
            { PropertyTypeNames.Double, typeof(NumberTypeConverter<double>) },
            { PropertyTypeNames.Single, typeof(NumberTypeConverter<float>) },
            { PropertyTypeNames.Int32, typeof(NumberTypeConverter<int>) },
            { PropertyTypeNames.UInt32, typeof(NumberTypeConverter<uint>) },
            { PropertyTypeNames.Int64, typeof(NumberTypeConverter<long>) },
            { PropertyTypeNames.UInt64, typeof(NumberTypeConverter<ulong>) },
            { PropertyTypeNames.Int16, typeof(NumberTypeConverter<short>) },
            { PropertyTypeNames.UInt16, typeof(NumberTypeConverter<ushort>) },
            { PropertyTypeNames.Enum, typeof(EnumTypeConverter) },
            { PropertyTypeNames.StringArray, typeof(ArrayTypeConverter) },
            { PropertyTypeNames.BooleanArray, typeof(BooleanArrayTypeConverter) },
            { PropertyTypeNames.ByteArray, typeof(NumberArrayTypeConverter<byte>) },
            { PropertyTypeNames.SByteArray, typeof(NumberArrayTypeConverter<sbyte>) },
            { PropertyTypeNames.DecimalArray, typeof(NumberArrayTypeConverter<decimal>) },
            { PropertyTypeNames.DoubleArray, typeof(NumberArrayTypeConverter<double>) },
            { PropertyTypeNames.SingleArray, typeof(NumberArrayTypeConverter<float>) },
            { PropertyTypeNames.Int32Array, typeof(NumberArrayTypeConverter<int>) },
            { PropertyTypeNames.UInt32Array, typeof(NumberArrayTypeConverter<uint>) },
            { PropertyTypeNames.Int64Array, typeof(NumberArrayTypeConverter<long>) },
            { PropertyTypeNames.UInt64Array, typeof(NumberArrayTypeConverter<ulong>) },
            { PropertyTypeNames.Int16Array, typeof(NumberArrayTypeConverter<short>) },
            { PropertyTypeNames.UInt16Array, typeof(NumberArrayTypeConverter<ushort>) }
        };

        internal static IPropertyTypeConverter GetTypeConverter(string typeString, string propertyName)
        {
            if (string.IsNullOrEmpty(typeString))
                return null;

            var type = GetConverterType(typeString);

            if (type == null)
                return null;

            if (ConvertersCache.TryGetValue(type, out var typeParser))
                return typeParser;

            typeParser = CreateTypeConverter(typeString, propertyName);
            ConvertersCache.Add(type, typeParser);
            return typeParser;
        }

        private static IPropertyTypeConverter CreateTypeConverter(string typeString, string propertyName)
        {
            var type = GetConverterType(typeString);
            return (IPropertyTypeConverter)type?.InvokeConstructor(new object[] { propertyName });
        }

        private static Type GetConverterType(string typeString) => 
            !string.IsNullOrEmpty(typeString) && PropertyTypeConverterTypeMap.TryGetValue(typeString, out var type) ? type : null;
    }
}
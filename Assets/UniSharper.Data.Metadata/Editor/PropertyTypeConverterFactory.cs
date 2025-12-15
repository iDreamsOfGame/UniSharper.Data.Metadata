// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using ReSharp.Patterns.Factory;
using UniSharperEditor.Data.Metadata.PropertyTypeConverters;

namespace UniSharperEditor.Data.Metadata
{
    internal class PropertyTypeConverterFactory : CachingFactoryTemplate<PropertyTypeConverterFactory, string, IPropertyTypeConverter>
    {
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
            { PropertyTypeNames.UnityVector2, typeof(UnityVector2TypeConverter) },
            { PropertyTypeNames.UnityVector2Int, typeof(UnityVector2IntTypeConverter) },
            { PropertyTypeNames.UnityVector3, typeof(UnityVector3TypeConverter) },
            { PropertyTypeNames.UnityVector3Int, typeof(UnityVector3IntTypeConverter) },
            { PropertyTypeNames.UnityVector4, typeof(UnityVector4TypeConverter) },
            { PropertyTypeNames.UnityRangeInt, typeof(UnityRangeIntTypeConverter) },
            { PropertyTypeNames.UnityQuaternion, typeof(UnityQuaternionTypeConverter) },
            { PropertyTypeNames.UnityRect, typeof(UnityRectTypeConverter) },
            { PropertyTypeNames.UnityRectInt, typeof(UnityRectIntTypeConverter) },
            { PropertyTypeNames.UnityColor, typeof(UnityColorTypeConverter) },
            { PropertyTypeNames.UnityColor32, typeof(UnityColor32TypeConverter) },
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
            { PropertyTypeNames.UInt16Array, typeof(NumberArrayTypeConverter<ushort>) },
            { PropertyTypeNames.UnityVector2Array, typeof(UnityVector2ArrayTypeConverter) },
            { PropertyTypeNames.UnityVector2IntArray, typeof(UnityVector2IntArrayTypeConverter) },
            { PropertyTypeNames.UnityVector3Array, typeof(UnityVector3ArrayTypeConverter) },
            { PropertyTypeNames.UnityVector3IntArray, typeof(UnityVector3IntArrayTypeConverter) },
            { PropertyTypeNames.UnityVector4Array, typeof(UnityVector4ArrayTypeConverter) },
            { PropertyTypeNames.UnityRangeIntArray, typeof(UnityRangeIntArrayTypeConverter) },
            { PropertyTypeNames.UnityQuaternionArray, typeof(UnityQuaternionArrayTypeConverter) },
            { PropertyTypeNames.UnityRectArray, typeof(UnityRectArrayTypeConverter) },
            { PropertyTypeNames.UnityRectIntArray, typeof(UnityRectIntArrayTypeConverter) },
            { PropertyTypeNames.UnityColorArray, typeof(UnityColorArrayTypeConverter) },
            { PropertyTypeNames.UnityColor32Array, typeof(UnityColor32ArrayTypeConverter) }
        };

        private PropertyTypeConverterFactory()
            : base(PropertyTypeConverterTypeMap)
        {
        }
    }
}
// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using UnityEngine;

namespace UniSharperEditor.Data.Metadata
{
    internal sealed class PropertyTypeNames
    {
        internal const string String = "string";

        internal const string Boolean = "bool";

        internal const string Byte = "byte";

        internal const string SByte = "sbyte";

        internal const string Decimal = "decimal";

        internal const string Double = "double";

        internal const string Single = "float";

        internal const string Int32 = "int";

        internal const string UInt32 = "uint";

        internal const string Int64 = "long";
        
        internal const string UInt64 = "ulong";

        internal const string Int16 = "short";

        internal const string UInt16 = "ushort";

        internal const string Enum = "enum";
        
        internal const string UnityVector2 = nameof(Vector2);

        internal const string UnityVector2Int = nameof(Vector2Int);
        
        internal const string UnityVector3 = nameof(Vector3);

        internal const string UnityVector3Int = nameof(Vector3Int);
        
        internal const string UnityVector4 = nameof(Vector4);
        
        internal const string UnityRangeInt = nameof(RangeInt);
        
        internal const string UnityQuaternion = nameof(Quaternion);
        
        internal const string UnityRect = nameof(Rect);

        internal const string UnityRectInt = nameof(RectInt);
        
        internal const string UnityColor = nameof(Color);

        internal const string UnityColor32 = nameof(Color32);

        internal const string StringArray = "string[]";

        internal const string BooleanArray = "bool[]";

        internal const string ByteArray = "byte[]";

        internal const string SByteArray = "sbyte[]";

        internal const string DecimalArray = "decimal[]";

        internal const string DoubleArray = "double[]";

        internal const string SingleArray = "float[]";

        internal const string Int32Array = "int[]";

        internal const string UInt32Array = "uint[]";

        internal const string Int64Array = "long[]";

        internal const string UInt64Array = "ulong[]";

        internal const string Int16Array = "short[]";

        internal const string UInt16Array = "ushort[]";

        internal static readonly string UnityVector2Array = $"{UnityVector2}[]";
        
        internal static readonly string UnityVector2IntArray = $"{UnityVector2Int}[]";

        internal static readonly string UnityVector3Array = $"{UnityVector3}[]";
        
        internal static readonly string UnityVector3IntArray = $"{UnityVector3Int}[]";
        
        internal static readonly string UnityVector4Array = $"{UnityVector4}[]";
        
        internal static readonly string UnityRangeIntArray = $"{UnityRangeInt}[]";
        
        internal static readonly string UnityQuaternionArray = $"{UnityQuaternion}[]";
        
        internal static readonly string UnityRectArray = $"{UnityRect}[]";
        
        internal static readonly string UnityRectIntArray = $"{UnityRectInt}[]";
        
        internal static readonly string UnityColorArray = $"{UnityColor}[]";
        
        internal static readonly string UnityColor32Array = $"{UnityColor32}[]";

        internal static bool IsUnityTypeArray(string typeString) 
            => typeString.Equals(UnityVector2Array) 
               || typeString.Equals(UnityVector2IntArray) 
               || typeString.Equals(UnityVector3Array)
               || typeString.Equals(UnityVector3IntArray)
               || typeString.Equals(UnityVector4Array)
               || typeString.Equals(UnityRangeIntArray)
               || typeString.Equals(UnityQuaternionArray)
               || typeString.Equals(UnityRectArray)
               || typeString.Equals(UnityRectIntArray)
               || typeString.Equals(UnityColorArray)
               || typeString.Equals(UnityColor32Array);
    }
}
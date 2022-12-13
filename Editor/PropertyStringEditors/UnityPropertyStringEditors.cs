// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System.Text;
using UniSharper;
using UnityEngine;

namespace UniSharperEditor.Data.Metadata.PropertyStringEditors
{
    internal class UnityVector2PropertyStringEditor : UnityTypePropertyStringEditor<Vector2>
    {
        protected override string ValuePropertyTypeName => PropertyTypeNames.SingleArray;
        
        protected override int NumberOfParameters => 2;
    }
    
    internal class UnityVector2IntPropertyStringEditor : UnityTypePropertyStringEditor<Vector2Int>
    {
        protected override string ValuePropertyTypeName => PropertyTypeNames.Int32Array;
        
        protected override int NumberOfParameters => 2;
    }
    
    internal class UnityVector3PropertyStringEditor : UnityTypePropertyStringEditor<Vector3>
    {
        protected override string ValuePropertyTypeName => PropertyTypeNames.SingleArray;
        
        protected override int NumberOfParameters => 3;
    }
    
    internal class UnityVector3IntPropertyStringEditor : UnityTypePropertyStringEditor<Vector3Int>
    {
        protected override string ValuePropertyTypeName => PropertyTypeNames.Int32Array;
        
        protected override int NumberOfParameters => 3;
    }
    
    internal class UnityVector4PropertyStringEditor : UnityTypePropertyStringEditor<Vector4>
    {
        protected override string ValuePropertyTypeName => PropertyTypeNames.SingleArray;
        
        protected override int NumberOfParameters => 4;
    }
    
    internal class UnityRectPropertyStringEditor : UnityTypePropertyStringEditor<Rect>
    {
        protected override string ValuePropertyTypeName => PropertyTypeNames.SingleArray;
        
        protected override int NumberOfParameters => 4;
    }
    
    internal class UnityRectIntPropertyStringEditor : UnityTypePropertyStringEditor<RectInt>
    {
        protected override string ValuePropertyTypeName => PropertyTypeNames.Int32Array;
        
        protected override int NumberOfParameters => 4;
    }
    
    internal class UnityRangeIntPropertyStringEditor : UnityTypePropertyStringEditor<RangeInt>
    {
        protected override string ValuePropertyTypeName => PropertyTypeNames.Int32Array;
        
        protected override int NumberOfParameters => 2;
    }
    
    internal class UnityQuaternionPropertyStringEditor : UnityTypePropertyStringEditor<Quaternion>
    {
        protected override string ValuePropertyTypeName => PropertyTypeNames.SingleArray;
        
        protected override int NumberOfParameters => 4;
    }
    
    internal class UnityColor32PropertyStringEditor : UnityTypePropertyStringEditor<Color32>
    {
        protected override string ValuePropertyTypeName => PropertyTypeNames.ByteArray;

        protected override int NumberOfParameters => 4;
    }
    
    internal class UnityColorPropertyStringEditor : UnityTypePropertyStringEditor<Color>
    {
        protected override string ValuePropertyTypeName => PropertyTypeNames.SingleArray;
        
        protected override int NumberOfParameters => 4;
    }
    
    internal abstract class UnityTypePropertyStringEditor<T> : PropertyStringEditor
    {
        protected abstract string ValuePropertyTypeName { get; }

        protected abstract int NumberOfParameters { get; }
        
        public override void Edit(StringBuilder stringBuilder, EntityPropertyRawInfo rawInfo)
        {
            // Add value property
            stringBuilder.AppendFormat(ScriptTemplate.ClassMemberFormatString.PropertyMember, rawInfo.Comment, ValuePropertyTypeName, rawInfo.PropertyName);
            
            stringBuilder.Append(PlayerEnvironment.WindowsNewLine)
                .Append(PlayerEnvironment.WindowsNewLine);
            
            // Add Unity type property
            var propertyType = typeof(T).FullName;
            var value = GenerateValueString(rawInfo.PropertyName);
            stringBuilder.AppendFormat(ScriptTemplate.ClassMemberFormatString.OnlyGetterPropertyMember, rawInfo.Comment, propertyType, rawInfo.Parameters[0], value);
        }

        private string GenerateValueString(string propertyName)
        {
            var valueStringBuilder = new StringBuilder("new(");
            for (var i = 0; i < NumberOfParameters; i++)
            {
                valueStringBuilder.AppendFormat($"{propertyName}[{i}]");
                if (i < NumberOfParameters - 1)
                    valueStringBuilder.AppendFormat(", ");
            }

            valueStringBuilder.AppendFormat(")");
            return valueStringBuilder.ToString();
        }
    }
}
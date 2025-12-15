// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System.Data;

namespace UniSharperEditor.Data.Metadata.PropertyRawInfoEditors
{
    internal class UnityVector2RawInfoEditor : UnityTypeRawInfoEditor
    {
        protected override string ValuePropertyTypeName => PropertyTypeNames.SingleArray;
        
        protected override int NumberOfConstructorParameters => 2;
    }

    internal class UnityVector2IntRawInfoEditor : UnityTypeRawInfoEditor
    {
        protected override string ValuePropertyTypeName => PropertyTypeNames.Int32Array;
        
        protected override int NumberOfConstructorParameters => 2;
    }

    internal class UnityVector3RawInfoEditor : UnityTypeRawInfoEditor
    {
        protected override string ValuePropertyTypeName => PropertyTypeNames.SingleArray;
        
        protected override int NumberOfConstructorParameters => 3;
    }

    internal class UnityVector3IntRawInfoEditor : UnityTypeRawInfoEditor
    {
        protected override string ValuePropertyTypeName => PropertyTypeNames.Int32Array;
        
        protected override int NumberOfConstructorParameters => 3;
    }

    internal class UnityVector4RawInfoEditor : UnityTypeRawInfoEditor
    {
        protected override string ValuePropertyTypeName => PropertyTypeNames.SingleArray;
        
        protected override int NumberOfConstructorParameters => 4;
    }

    internal class UnityRangeIntRawInfoEditor : UnityTypeRawInfoEditor
    {
        protected override string ValuePropertyTypeName => PropertyTypeNames.Int32Array;
        
        protected override int NumberOfConstructorParameters => 2;
    }

    internal class UnityQuaternionRawInfoEditor : UnityTypeRawInfoEditor
    {
        protected override string ValuePropertyTypeName => PropertyTypeNames.SingleArray;
        
        protected override int NumberOfConstructorParameters => 4;
    }

    internal class UnityRectRawInfoEditor : UnityTypeRawInfoEditor
    {
        protected override string ValuePropertyTypeName => PropertyTypeNames.SingleArray;
        
        protected override int NumberOfConstructorParameters => 4;
    }

    internal class UnityRectIntRawInfoEditor : UnityTypeRawInfoEditor
    {
        protected override string ValuePropertyTypeName => PropertyTypeNames.Int32Array;
        
        protected override int NumberOfConstructorParameters => 4;
    }

    internal class UnityColorRawInfoEditor : UnityTypeRawInfoEditor
    {
        protected override string ValuePropertyTypeName => PropertyTypeNames.SingleArray;
        
        protected override int NumberOfConstructorParameters => 4;
    }

    internal class UnityColor32RawInfoEditor : UnityTypeRawInfoEditor
    {
        protected override string ValuePropertyTypeName => PropertyTypeNames.ByteArray;
        
        protected override int NumberOfConstructorParameters => 4;
    }

    internal abstract class UnityTypeRawInfoEditor : PropertyRawInfoEditor
    {
        protected abstract string ValuePropertyTypeName { get; }
        
        protected abstract int NumberOfConstructorParameters { get; }
        
        public override EntityPropertyRawInfo Edit(MetadataAssetSettings settings, 
            DataTable table, 
            int column, 
            string comment, 
            string propertyType, 
            string propertyName)
        {
            var parameters = new object[]
            {
                ValuePropertyTypeName,
                propertyName,
                NumberOfConstructorParameters
            };
            propertyName = $"{propertyName}Value";
            return new EntityPropertyRawInfo(column, comment, propertyType, propertyName, parameters);
        }
    }
}
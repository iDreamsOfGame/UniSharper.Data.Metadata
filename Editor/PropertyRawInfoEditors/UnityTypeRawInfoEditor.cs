// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System.Data;

namespace UniSharperEditor.Data.Metadata.EntityRawInfoEditors
{
    internal class UnityTypeRawInfoEditor : PropertyRawInfoEditor
    {
        public UnityTypeRawInfoEditor(MetadataAssetSettings settings)
            : base(settings)
        {
        }

        public override EntityPropertyRawInfo Edit(DataTable table, int column, string comment, string propertyType, string propertyName)
        {
            var parameters = new object[1];
            parameters[0] = propertyName;
            propertyName = $"{propertyName}Value";
            return new EntityPropertyRawInfo(comment, propertyType, propertyName, parameters);
        }
    }
}
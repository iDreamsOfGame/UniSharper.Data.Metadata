// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System.Data;

namespace UniSharperEditor.Data.Metadata.PropertyRawInfoEditors
{
    internal interface IPropertyRawInfoEditor
    {
        EntityPropertyRawInfo Edit(MetadataAssetSettings settings, DataTable dataTable, int column, string comment, string propertyType, string propertyName);
    }
}
// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

namespace UniSharperEditor.Data.Metadata.EntityRawInfoEditors
{
    internal interface IPropertyRawInfoEditor
    {
        EntityPropertyRawInfo Edit(int column, string comment, string propertyType, string propertyName);
    }
}
// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System.Data;

namespace UniSharperEditor.Data.Metadata.EntityRawInfoEditors
{
    internal abstract class PropertyRawInfoEditor : IPropertyRawInfoEditor
    {
        internal PropertyRawInfoEditor(MetadataAssetSettings settings, DataTable table)
        {
            Settings = settings;
            Table = table;
        }

        protected MetadataAssetSettings Settings { get; }

        protected DataTable Table { get; }

        public abstract EntityPropertyRawInfo Edit(int column, string comment, string propertyType, string propertyName);
    }
}
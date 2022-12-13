// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System.Text;

namespace UniSharperEditor.Data.Metadata.PropertyStringEditors
{
    internal interface IPropertyStringEditor
    {
        void Edit(StringBuilder stringBuilder, EntityPropertyRawInfo rawInfo);
    }
}
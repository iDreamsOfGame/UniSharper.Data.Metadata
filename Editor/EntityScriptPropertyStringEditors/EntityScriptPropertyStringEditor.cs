// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System.Text;

namespace UniSharperEditor.Data.Metadata.EntityScriptPropertyStringEditors
{
    internal abstract class EntityScriptPropertyStringEditor : IEntityScriptPropertyStringEditor
    {
        public abstract void Edit(StringBuilder stringBuilder, EntityPropertyRawInfo rawInfo);
    }
}
// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

namespace UniSharperEditor.Data.Metadata.Converters
{
    internal interface IPropertyTypeConverter
    {
        object Parse(char arrayElementSeparator, string value, params object[] parameters);
    }
}
// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Text;

namespace UniSharper.Data.Metadata
{
    /// <summary>
    /// The abstract class for metadata entity.
    /// </summary>
    public abstract class MetadataEntity
    {
        #region Methods

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents this instance.</returns>
        public override string ToString()
        {
            var outputBuilder = new StringBuilder();
            var propertiesBuilder = new StringBuilder();
            var map = this.GetPropertyValuePairs();

            foreach (var output in map.Select(kvp => $"{kvp.Key} = {kvp.Value}, "))
            {
                propertiesBuilder.Append(output);
            }

            outputBuilder.AppendFormat("{0}({1})", base.ToString(), propertiesBuilder.ToString(0, propertiesBuilder.Length - 2));
            return outputBuilder.ToString();
        }

        #endregion Methods
    }
}
// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
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
            StringBuilder sb = new StringBuilder();
            StringBuilder propsSb = new StringBuilder();
            Dictionary<string, object> map = this.GetPropertyValuePairs();

            foreach (KeyValuePair<string, object> kvp in map)
            {
                string output = string.Format("{0} = {1}, ", kvp.Key, kvp.Value);
                propsSb.Append(output);
            }

            sb.AppendFormat("{0}({1})", base.ToString(), propsSb.ToString(0, propsSb.Length - 2));
            return sb.ToString();
        }

        #endregion Methods
    }
}
// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

namespace UniSharper.Data.Metadata.Providers
{
    /// <summary>
    /// Provides the methods to compress and decompress database.
    /// </summary>
    public interface IDatabaseCompressionProvider
    {
        /// <summary>
        /// Compresses the input data.
        /// </summary>
        /// <param name="input">The uncompressed data. </param>
        /// <returns>The compressed data. </returns>
        byte[] Compress(byte[] input);

        /// <summary>
        /// Decompresses the input data.
        /// </summary>
        /// <param name="input">The compressed data. </param>
        /// <returns>The uncompressed data. </returns>
        byte[] Decompress(byte[] input);
    }
}
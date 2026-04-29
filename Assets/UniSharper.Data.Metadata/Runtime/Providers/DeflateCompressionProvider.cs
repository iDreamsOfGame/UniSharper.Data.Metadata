// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

using ReSharp.Compression;

namespace UniSharper.Data.Metadata.Providers
{
    /// <summary>
    /// Deflate algorithm implementation of <see cref="IDatabaseCompressionProvider"/>.
    /// </summary>
    internal class DeflateCompressionProvider : IDatabaseCompressionProvider
    {
        public byte[] Compress(byte[] input) => Deflate.Compress(input);

        public byte[] Decompress(byte[] input) => Deflate.Decompress(input);
    }
}
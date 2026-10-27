// Copyright (c) Jerry Lee. All rights reserved. Licensed under the MIT License.
// See LICENSE in the project root for license information.

#if ENABLE_K4OS_LZ4_PACKAGE
using K4os.Compression.LZ4;
using K4os.Compression.LZ4.Utilities;

namespace UniSharper.Data.Metadata.Providers
{
    /// <summary>
    /// Provides compression and decompression functionality using the LZ4 algorithm.
    /// </summary>
    public class LZ4CompressionProvider : IDatabaseCompressionProvider
    {
        public byte[] Compress(byte[] input) => LZ4Compression.Compress(input, LZ4Level.L12_MAX);

        public byte[] Decompress(byte[] input) => LZ4Compression.Decompress(input);
    }
}
#endif
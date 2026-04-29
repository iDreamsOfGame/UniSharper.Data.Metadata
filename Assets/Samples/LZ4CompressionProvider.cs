using K4os.Compression.LZ4;
using K4os.Compression.LZ4.Utilities;
using UniSharper.Data.Metadata.Providers;

namespace UniSharper.Data.Metadata.Samples
{
    internal class LZ4CompressionProvider : IDatabaseCompressionProvider
    {
        public byte[] Compress(byte[] input) => LZ4Compression.Compress(input, LZ4Level.L12_MAX);

        public byte[] Decompress(byte[] input) => LZ4Compression.Decompress(input);
    }
}
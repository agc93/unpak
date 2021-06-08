using System.Collections.Generic;
using System.IO;

namespace UnPak.Core.Compression
{
    public interface ICompressionProvider
    {
        long BlockSize { get; }
        CompressionMethod Method { get; }
        IEnumerable<KeyValuePair<CompressionBlock, byte[]>> CompressFile(Stream inputStream, ulong startOffset);

        //byte[] DecompressBlocks(IEnumerable<KeyValuePair<CompressionBlock, byte[]>> blocks, Stream inputData);
        //public byte[] DecompressBytes(byte[] bytes);
    }
}
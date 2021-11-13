using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace UnPak.Core.Compression
{
    public class ZlibCompressionProvider : CompressionProviderBase
    {
        public ZlibCompressionProvider(PackageCompression compression) : base(compression) {
            
        }
        
        public override IEnumerable<KeyValuePair<CompressionBlock, byte[]>> CompressFile(Stream inputData, ulong startOffset) {
            // var chunks = ReadCompressedChunks(inputData).ToList();
            var chunks = ReadCompressedChunks(inputData);
            foreach (var chunk in chunks) {
                var endOffset = startOffset + (ulong) chunk.Length;
                var block = new KeyValuePair<CompressionBlock, byte[]>(
                    new CompressionBlock {StartOffset = startOffset, EndOffset = endOffset},
                    chunk);
                startOffset = endOffset;
                yield return block;
            }
        }
        
        private IEnumerable<byte[]> ReadCompressedChunks(Stream inFs) {
            foreach (var chunk in ReadFileChunks(inFs)) {
                using var ms = new MemoryStream(chunk);
                using var tgtStr = new MemoryStream(chunk.Length);
                using var compStr = new DeflaterOutputStream(tgtStr, new Deflater((int) Deflater.CompressionLevel.DEFAULT_COMPRESSION));
                ms.CopyTo(compStr);
                compStr.Flush();
                compStr.Close();
                yield return tgtStr.ToArray();
            }
        }

        IEnumerable<byte[]> DecompressBlocks(IEnumerable<KeyValuePair<CompressionBlock, byte[]>> blocks) {
            foreach (var (compressionBlock, bytes) in blocks) {
                var blockSize = (int) (compressionBlock.EndOffset - compressionBlock.StartOffset);
                yield return DecompressBytes(bytes);
            }
        }

        /*public override byte[] DecompressBytes(byte[] bytes) {
            using var memStream = new MemoryStream(bytes);
            using var tgtStr = new MemoryStream();
            using var dfStream = new DeflaterOutputStream(tgtStr);
            memStream.CopyTo(dfStream);
            dfStream.Flush();
            return tgtStr.ToArray();
        }*/

        public static byte[] DecompressBytes(byte[] bytes) {
            using var memStream = new MemoryStream(bytes);
            using var tgtStr = new MemoryStream();
            using var dfStream = new DeflaterOutputStream(tgtStr);
            memStream.CopyTo(dfStream);
            dfStream.Flush();
            return tgtStr.ToArray();
        }
    }
}
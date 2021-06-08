using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace UnPak.Core.Compression
{
    public class RawCompressionProvider : ICompressionProvider
    {
        private readonly PackageCompression _compression;
        public long BlockSize { get; }
        public CompressionMethod Method => _compression.Method;

        public RawCompressionProvider(PackageCompression compression) {
            _compression = compression;
            BlockSize = GetBlockSize();
        }

        public IEnumerable<KeyValuePair<CompressionBlock, byte[]>> CompressFile(Stream inFs, ulong startOffset) {
            var chunks = ReadCompressedChunks(inFs).ToList();
            for (int i = 0; i < chunks.Count; i++) {
                var chunk = chunks[i];
                var endOffset = startOffset + (ulong) chunk.Length;
                var block = new KeyValuePair<CompressionBlock, byte[]>(
                    new CompressionBlock {StartOffset = startOffset, EndOffset = endOffset},
                    chunk);
                startOffset = endOffset;
                yield return block;
            }
            
        }

        /*public IEnumerable<byte[]> CompressChunks() {
            using var fs = _file.OpenRead();
            var chunk = new byte[BlockSize];
            while (true)
            {
                int index = 0;
                // There are various different ways of structuring this bit of code.
                // Fundamentally we're trying to keep reading in to our chunk until
                // either we reach the end of the stream, or we've read everything we need.
                while (index < chunk.Length)
                {
                    // using var tgtStr = new MemoryStream((int) fs.Length);
                    
                    var bytesRead = fs.Read(chunk, index, chunk.Length - index);
                    
                    if (bytesRead == 0)
                    {
                        break;
                    }
                    index += bytesRead;
                }
                if (index != 0) // Our previous chunk may have been the last one
                {
                    using var ms = new MemoryStream(chunk);
                    using var tgtStr = new MemoryStream(chunk.Length);
                    using var compStr = new DeflateStream(tgtStr, CompressionMode.Compress, true);
                    ms.CopyTo(compStr);
                    compStr.Flush();
                    compStr.Close();
                    yield return tgtStr.ToArray();
                }
                if (index != chunk.Length) // We didn't read a full chunk: we're done
                {
                    yield break;
                }
            }
        }*/

        private IEnumerable<byte[]> ReadCompressedChunks(Stream inFs) {
            var allChunks = new List<byte[]>();
            //var chunks = GetReadChunks().ToList();
            foreach (var chunk in GetReadChunks(inFs)) {
                using var ms = new MemoryStream(chunk);
                using var tgtStr = new MemoryStream(chunk.Length);
                using var compStr = new DeflateStream(tgtStr, CompressionMode.Compress, true);
                ms.CopyTo(compStr);
                compStr.Flush();
                compStr.Close();
                allChunks.Add(tgtStr.ToArray());
            }

            return allChunks;
        }

        private IEnumerable<byte[]> GetReadChunks(Stream fs) {
            int n;
            var buffer = new byte[BlockSize];
            while ((n = fs.Read(buffer, 0, (int) BlockSize)) != 0)  {
                if (n == BlockSize) {
                    yield return buffer;
                }
                else {
                    yield return buffer[Range.EndAt(n)];
                }
            }
        }

        /*private IEnumerable<byte[]> ReadChunks() {
            var chunk = new byte[BlockSize];
            using (var fs = _file.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                // var n = fs.Read(lengthBytes, 0, sizeof (int));  // Read block size.
                //
                // if (n == 0)      // End of file.
                //     yield break;
                //
                // if (n != sizeof(int))
                //     throw new InvalidOperationException("Invalid header");
                
                var buffer = new byte[BlockSize];
                var n = fs.Read(buffer, 0, (int) BlockSize);

                if (n == 0) yield break;
                if (n == BlockSize) {
                    yield return buffer;
                }
                else {
                    yield return buffer[Range.EndAt(n)];
                }
            }
        }*/

        private long GetBlockSize() {
            return _compression.BlockSize;
        }
    }
}
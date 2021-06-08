using System;
using System.Collections.Generic;
using System.IO;

namespace UnPak.Core.Compression
{
    public abstract class CompressionProviderBase : ICompressionProvider
    {
        private readonly PackageCompression _compression;

        protected CompressionProviderBase(PackageCompression compression) {
            _compression = compression;
        }

        public Stream GetStream(FileInfo fi) {
            return fi.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public virtual long BlockSize => _compression.BlockSize;
        public virtual CompressionMethod Method => _compression.Method;
        public abstract IEnumerable<KeyValuePair<CompressionBlock, byte[]>> CompressFile(Stream inputStream, ulong startOffset);
        //public abstract byte[] DecompressBytes(byte[] bytes);

        protected IEnumerable<byte[]> ReadFileChunks(Stream fs) {
            // using var fs = _file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
            int n;
            var buffer = new byte[BlockSize];
            while ((n = fs.Read(buffer, 0, (int) BlockSize)) != 0)  {
                if (n == BlockSize) {
                    yield return buffer;
                }
                else {
                    yield return buffer[Range.EndAt(n-1)];
                }
            }
        }
    }
}
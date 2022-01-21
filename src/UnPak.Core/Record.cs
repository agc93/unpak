using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnPak.Core.Compression;

namespace UnPak.Core
{
    public class Record
    {
        public Record(ulong headerSize) {
            HeaderSize = headerSize;
        }
        public string FileName { get; init; }
        public ulong RecordOffset { get; init; }
        public ulong CompressedSize { get; init; }
        public ulong RawSize { get; init; }
        public CompressionMethod CompressionMethod { get; init; }
        public byte[] Hash { get; init; }
        public List<CompressionBlock>? Blocks { get; init; } = new List<CompressionBlock>();
        public bool Encrypted { get; init; }
        public uint CompressionBlockSize { get; init; }
        public ulong HeaderSize { get; }
        public long DataOffset => (long) (RecordOffset + HeaderSize);

        private void Unpack(Stream pakFile, Stream outStream) {
            switch (CompressionMethod) {
                case CompressionMethod.None:
                {
                    
                    pakFile.Seek(DataOffset, SeekOrigin.Begin);
                    pakFile.CopyStream(outStream, (long) RawSize);
                    outStream.Flush();
                    break;
                }
                case CompressionMethod.Zlib:
                {
                    if (Blocks == null || !Blocks.Any()) {
                        throw new InvalidDataException("No compression blocks available for compressed record!");
                    }
                    //pakFile.Seek(DataOffset, SeekOrigin.Begin);
                    //using var bReader = new BinaryReader(pakFile);
                    
                    foreach (var compressionBlock in Blocks) {
                        var blockSize = (int) (compressionBlock.EndOffset - compressionBlock.StartOffset);
                        pakFile.Seek(DataOffset + (long) compressionBlock.StartOffset, SeekOrigin.Begin);
                        using var memStream = new MemoryStream(blockSize);
                        pakFile.CopyStream(memStream, blockSize);
                        outStream.Write(ZlibCompressionProvider.DecompressBytes(memStream.ToArray()));
                        // using var dfStream = new DeflateStream(memStream, CompressionMode.Decompress, true);
                        // dfStream.CopyTo(outFs);
                    }
                    outStream.Flush();
                    break;
                }
                default:
                    throw new NotImplementedException();
            }
        }

        public Stream Unpack(Stream pakFile) {
            var memStream = new MemoryStream((int) RawSize);
            Unpack(pakFile, memStream);
            memStream.Seek(0, SeekOrigin.Begin);
            return memStream;
        }

        public FileInfo Unpack(Stream pakFile, DirectoryInfo unpackRoot) {
            // using var sr = new StreamReader(pakFile, leaveOpen:true);
            var dirName = Path.GetDirectoryName(FileName);
            if (dirName == null) {
                throw new ArgumentException($"Could not determine directory name from file name: {FileName}");
            }
            var tgtBasePath = Path.Combine(unpackRoot.FullName, dirName);
            var di = Directory.CreateDirectory(tgtBasePath);
            var tgtPath = Path.Combine(di.FullName, Path.GetFileName(FileName));
            using var outFs = new FileStream(tgtPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            Unpack(pakFile, outFs);
            outFs.Flush(true);

            return new FileInfo(tgtPath);
        }
    }
}
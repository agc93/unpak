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

        public FileInfo Unpack(FileStream pakFile, DirectoryInfo unpackRoot) {
            // using var sr = new StreamReader(pakFile, leaveOpen:true);
            var tgtBasePath = Path.Combine(unpackRoot.FullName, Path.GetDirectoryName(FileName));
            var di = Directory.CreateDirectory(tgtBasePath);
            var tgtPath = Path.Combine(di.FullName, Path.GetFileName(FileName));
            using var outFs = new FileStream(tgtPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            switch (CompressionMethod) {
                case CompressionMethod.None:
                {
                    
                    pakFile.Seek((long) DataOffset, SeekOrigin.Begin);
                    pakFile.CopyStream(outFs, (long) RawSize);
                    outFs.Flush();
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
                        outFs.Write(ZlibCompressionProvider.DecompressBytes(memStream.ToArray()));
                        // using var dfStream = new DeflateStream(memStream, CompressionMode.Decompress, true);
                        // dfStream.CopyTo(outFs);
                    }
                    outFs.Flush();
                    break;
                }
                default:
                    throw new NotImplementedException();
            }
            outFs.Flush(true);

            return new FileInfo(tgtPath);
        }
    }
}
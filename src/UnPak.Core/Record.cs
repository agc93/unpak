using System;
using System.Collections.Generic;
using System.IO;

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
        public List<Block> Blocks { get; } = new List<Block>();
        public bool Encrypted { get; init; }
        public uint CompressionBlockSize { get; init; }
        public ulong HeaderSize { get; }
        public long DataOffset => (long) (RecordOffset + HeaderSize);

        public FileInfo Unpack(FileStream pakFile, DirectoryInfo unpackRoot) {
            // using var sr = new StreamReader(pakFile, leaveOpen:true);
            var tgtBasePath = Path.Combine(unpackRoot.FullName, Path.GetDirectoryName(FileName));
            var di = Directory.CreateDirectory(tgtBasePath);
            var tgtPath = Path.Combine(di.FullName, Path.GetFileName(FileName));
            if (CompressionMethod == CompressionMethod.None) {
                using var outFs = new FileStream(tgtPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                pakFile.Seek((long) DataOffset, SeekOrigin.Begin);
                pakFile.CopyStream(outFs, (long) RawSize);
                outFs.Flush();
            }
            else {
                throw new NotImplementedException("Compression not supported!");
            }

            return new FileInfo(tgtPath);
        }
    }
}
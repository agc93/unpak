using System;
using System.Collections.Generic;
using System.IO;

namespace UnPak.Core
{
    public interface IPakFormat
    {
        public SupportedOperations Supports { get; }
        public IEnumerable<int> Versions { get; }
        public uint? Magic { get; }
        public Record ReadRecord(BinaryReader binaryReader, string fileName);
        public byte[] WriteRecord(BinaryWriter binaryWriter, ArchiveFile file, bool isEncrypted);
        public Record WriteCompressedRecord(BinaryWriter binaryWriter, FileInfo file, bool isEncrypted, CompressionMethod compression, uint compressionBlockSize);
    }

    [Flags]
    public enum SupportedOperations
    {
        None = 0,
        Pack = 1,
        Unpack = 2
    }
}
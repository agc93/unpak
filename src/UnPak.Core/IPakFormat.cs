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
        public byte[] WriteRecord(BinaryWriter binaryWriter, ArchiveFile file, bool isEncrypted, PackageCompression? compression);
    }

    [Flags]
    public enum SupportedOperations
    {
        None = 0,
        Write = 1,
        Read = 2
    }
}
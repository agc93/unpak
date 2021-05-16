using System.IO;

namespace UnPak.Core
{
    public interface IPakFormat
    {
        public int Version { get; }
        public uint? Magic { get; }
        public Record ReadRecord(BinaryReader binaryReader, string fileName);
        public byte[] WriteRecord(BinaryWriter binaryWriter, ArchiveFile file, bool isEncrypted);
        public Record WriteCompressedRecord(BinaryWriter binaryWriter, FileInfo file, bool isEncrypted, CompressionMethod compression, uint compressionBlockSize);
    }
}
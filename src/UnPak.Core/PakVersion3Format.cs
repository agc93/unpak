using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnPak.Core.Crypto;

namespace UnPak.Core
{
    public abstract class PakFormat : IPakFormat
    {
        public abstract SupportedOperations Supports { get; }
        public abstract IEnumerable<int> Versions { get; }
        public virtual uint? Magic => null;
        public abstract Record ReadRecord(BinaryReader binaryReader, string fileName);
        public abstract byte[] WriteRecord(BinaryWriter binaryWriter, ArchiveFile file, bool isEncrypted);

        public abstract Record WriteCompressedRecord(BinaryWriter binaryWriter, FileInfo file, bool isEncrypted, CompressionMethod compression,
            uint compressionBlockSize);
    }

    public class PakVersion4Format : IPakFormat
    {
        public SupportedOperations Supports => SupportedOperations.None;
        public IEnumerable<int> Versions => new[] {4};
        public uint? Magic => null;
        public Record ReadRecord(BinaryReader streamReader, string fileName) {
            var offset = streamReader.ReadUInt64();
            var compressedSize = streamReader.ReadUInt64();
            var rawSize = streamReader.ReadUInt64();
            var compressionVal = streamReader.ReadUInt32();
            var compression = Enum.Parse<CompressionMethod>(compressionVal.ToString());
            var hash = streamReader.ReadBytes(20);
            //var blocks = GetBlocks(compression, streamReader);
            var encrypted = streamReader.ReadChar();
            var blockSize = streamReader.ReadUInt32();
            var _ = streamReader.ReadUInt32();
            return new Record(53) {
                FileName = fileName,
                RecordOffset = offset,
                CompressedSize = compressedSize,
                RawSize = rawSize,
                CompressionMethod = compression,
                Hash = hash,
                Encrypted = encrypted != 0,
                CompressionBlockSize = blockSize
            };
        }

        public byte[] WriteRecord(BinaryWriter binaryWriter, ArchiveFile archiveFile, bool isEncrypted) {
            throw new NotImplementedException();
        }

        public Record WriteCompressedRecord(BinaryWriter binaryWriter, FileInfo file, bool isEncrypted, CompressionMethod compression,
            uint compressionBlockSize) {
            throw new NotImplementedException();
        }
    }

    public class PakVersion3Format : IPakFormat
    {
        private readonly IHashProvider _hashProvider;

        public PakVersion3Format(IHashProvider hashProvider) {
            _hashProvider = hashProvider;
        }
        public SupportedOperations Supports => SupportedOperations.Pack | SupportedOperations.Unpack;
        public virtual IEnumerable<int> Versions => new[] {3, 7};
        public uint? Magic => null;

        public Record ReadRecord(BinaryReader streamReader, string fileName) {
            var offset = streamReader.ReadUInt64();
            var compressedSize = streamReader.ReadUInt64();
            var rawSize = streamReader.ReadUInt64();
            var compressionVal = streamReader.ReadUInt32();
            var compression = Enum.Parse<CompressionMethod>(compressionVal.ToString());
            var hash = streamReader.ReadBytes(20);
            var blocks = GetBlocks(compression, streamReader);
            var encrypted = streamReader.ReadChar();
            var blockSize = streamReader.ReadUInt32();
            return new Record(53) {
                FileName = fileName,
                RecordOffset = offset,
                CompressedSize = compressedSize,
                RawSize = rawSize,
                CompressionMethod = compression,
                Hash = hash,
                Encrypted = encrypted != 0,
                CompressionBlockSize = blockSize
            };
        }

        public Record WriteCompressedRecord(BinaryWriter binaryWriter, FileInfo file, bool isEncrypted, CompressionMethod compression,
            uint compressionBlockSize) {
            throw new NotImplementedException();
        }

        public byte[] WriteRecord(BinaryWriter binaryWriter, ArchiveFile archiveFile, bool isEncrypted) {
            var file = archiveFile.File;
            var curr = binaryWriter.BaseStream.Position;
            //var size = file.Length;
            //var recordBytes = new byte[53 + file.Length];
            var hash = _hashProvider.GetSha1Hash(file);
            using var tgtStream = new MemoryStream((int) (53 + file.Length));
            using var writer = new BinaryWriter(tgtStream, Encoding.ASCII, true);
            var header = GetIndexRecord(0, (file.Length, file.Length), hash, CompressionMethod.None, isEncrypted, 0);
            writer.Write(header);
            writer.WriteFile(file);
            writer.Flush();
            //tgtStream.CopyTo(binaryWriter.BaseStream);
            var dataRecord = tgtStream.ToArray();
            binaryWriter.Write(dataRecord);
            
            var indexRecord = GetIndexRecord(curr, (file.Length, file.Length), hash, CompressionMethod.None, isEncrypted, 0);
            return indexRecord;
        }

        private byte[] GetIndexRecord(long offset, (long compressedLength, long rawLength) size, byte[] hash, CompressionMethod compressionMethod,
            bool isEncrypted, uint compressionBlockSize) {
            using var tgtStream = new MemoryStream();
            using var writer = new BinaryWriter(tgtStream, Encoding.ASCII);
            writer.WriteUInt64((ulong) offset);
            writer.WriteInt64(size.compressedLength);
            writer.WriteInt64(size.rawLength);
            writer.WriteUInt32((uint) compressionMethod); // compression method
            writer.Write(hash);
            writer.Write((byte)(isEncrypted ? 0x01 : 0x00)); //encryption
            writer.WriteUInt32(compressionBlockSize); //compression block size
            writer.Flush();
            return tgtStream.ToArray();
        }

        private Dictionary<int, int> GetBlocks(CompressionMethod method, BinaryReader streamReader) {
            switch (method) {
                case CompressionMethod.None:
                    return new Dictionary<int, int>();
                case CompressionMethod.Zlib:
                    var blockCount = streamReader.ReadUInt32();
                    throw new NotImplementedException();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(method), method, null);
            }
        }
    }
}
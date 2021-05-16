using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace UnPak.Core
{
    public class PakVersion3Format : IPakFormat
    {
        public int Version => 3;
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
            var size = file.Length;
            var recordBytes = new byte[53 + file.Length];
            var hash = GetSha1Hash(file);
            using var tgtStream = new MemoryStream((int) (53 + file.Length));
            using var writer = new BinaryWriter(tgtStream, Encoding.ASCII, true);
            var header = GetIndexRecord(0, (file.Length, file.Length), hash, CompressionMethod.None, isEncrypted, 0);
            writer.Write(header);
            writer.WriteFile(file);
            writer.Flush();
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

        private byte[] GetSha1Hash(FileInfo fi, Encoding encoding = null) {
            encoding ??= Encoding.ASCII;
            using var fs = fi.OpenRead();
            using var sha1 = new SHA1Managed();
            var hash = sha1.ComputeHash(fs);
            // return encoding.GetString()
            return hash;
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
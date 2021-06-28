using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnPak.Core.Compression;
using UnPak.Core.Crypto;
using static UnPak.Core.FormatHelpers;

namespace UnPak.Core
{
    public abstract class PakFormat : IPakFormat
    {
        public abstract SupportedOperations Supports { get; }
        public abstract IEnumerable<int> Versions { get; }
        public virtual uint? Magic => null;
        public abstract Record ReadRecord(BinaryReader binaryReader, string fileName);
        public abstract byte[] WriteRecord(BinaryWriter binaryWriter, ArchiveFile file, bool isEncrypted, PackageCompression? compression);
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

        public byte[] WriteRecord(BinaryWriter binaryWriter, ArchiveFile file, bool isEncrypted, PackageCompression? compression) {
            throw new NotImplementedException();
        }
    }

    public class PakVersion3Format : IPakFormat
    {
        private readonly IHashProvider _hashProvider;

        public PakVersion3Format(IHashProvider hashProvider) {
            _hashProvider = hashProvider;
        }
        public SupportedOperations Supports => SupportedOperations.Write | SupportedOperations.Read;
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
            var compressionBlocks = blocks.ToList();
            return new Record(53) {
                FileName = fileName,
                RecordOffset = offset,
                CompressedSize = compressedSize,
                RawSize = rawSize,
                CompressionMethod = compression,
                Hash = hash,
                Encrypted = encrypted != 0,
                CompressionBlockSize = blockSize,
                Blocks = compressionBlocks.Any() ? compressionBlocks.ToList() : null
            };
        }

        public byte[] WriteRecord(BinaryWriter binaryWriter, ArchiveFile archiveFile, bool isEncrypted, PackageCompression? compression) {
            var file = archiveFile.File;
            var curr = binaryWriter.BaseStream.Position;
            //var size = file.Length;
            //var recordBytes = new byte[53 + file.Length];
            var hash = _hashProvider.GetSha1Hash(file);
            using var tgtStream = new MemoryStream((int) (53 + file.Length));
            using var writer = new BinaryWriter(tgtStream, Encoding.ASCII, true);
            
            switch (compression?.Method) {
                case null:
                case CompressionMethod.None:
                {
                    // using var tgtStream = new MemoryStream((int) (53 + file.Length));
                    // using var writer = new BinaryWriter(tgtStream, Encoding.ASCII, true);
                    var header = GetIndexRecord(0, (file.Length, file.Length), hash, compression?.Method ?? CompressionMethod.None, 0, isEncrypted);
                    writer.Write(header);
                    writer.WriteFile(file);
                    writer.Flush();
                    tgtStream.CopyTo(binaryWriter.BaseStream);
                     var dataRecord = tgtStream.ToArray();
                     binaryWriter.Write(dataRecord);
            
                    var indexRecord = GetIndexRecord(curr, (file.Length, file.Length), hash, CompressionMethod.None, 0, isEncrypted);
                    return indexRecord;
                }
                case CompressionMethod.Zlib:
                {
                    var zlib = new ZlibCompressionProvider(compression);
                    using var fs = zlib.GetStream(file);
                    var compressedData = zlib.CompressFile(fs, 0).ToList();
                    var compressedSize = compressedData.Sum(d => d.Value.Length);
                    var blocks = compressedData.Select(c => c.Key).ToList();
                    var fileLength = ((long) compressedSize, file.Length);
                    var firstOffset = (ulong) (curr + 53 + 4 + blocks.Count * 16);
                    // that's the current position + header + block count + (each block start:end pair is 16 bytes)
                    blocks = blocks.OffsetBy(firstOffset).ToList();
                    var compDataStr = new MemoryStream();
                    foreach (var pair in compressedData) {
                        compDataStr.Write(pair.Value);
                    }
                    var blockHash = _hashProvider.GetSha1Hash(compDataStr);
                    var compHeader = GetIndexRecord(0, fileLength, blockHash,
                        compression.Method, (uint) zlib.BlockSize, isEncrypted, blocks);
                    writer.Write(compHeader);
                    writer.Write(compDataStr.ToArray());
                    var cDataRecord = tgtStream.ToArray();
                    binaryWriter.Write(cDataRecord);
                    var compRecord = GetIndexRecord(curr, fileLength, blockHash, zlib.Method, (uint) zlib.BlockSize,
                        isEncrypted, blocks);
                    return compRecord;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(compression));
            }
        }

        private byte[] GetIndexRecord(long offset, (long compressedLength, long rawLength) size, byte[] hash, CompressionMethod compressionMethod, uint compressionBlockSize,
            bool isEncrypted, IEnumerable<CompressionBlock> blocks = null) {
            using var tgtStream = new MemoryStream();
            using var writer = new BinaryWriter(tgtStream, Encoding.ASCII);
            writer.WriteUInt64((ulong) offset);
            writer.WriteInt64(size.compressedLength);
            writer.WriteInt64(size.rawLength);
            writer.WriteUInt32((uint) compressionMethod); // compression method
            writer.Write(hash);
            if (compressionMethod != CompressionMethod.None && blocks != null) {
                writer.WriteUInt32((uint) blocks.Count());
                foreach (var block in blocks) {
                    writer.WriteUInt64(block.StartOffset);
                    writer.WriteUInt64(block.EndOffset);
                }
            }
            writer.Write((byte)(isEncrypted ? 0x01 : 0x00)); //encryption
            writer.WriteUInt32(compressionBlockSize); //compression block size
            writer.Flush();
            return tgtStream.ToArray();
        }

        
    }
}
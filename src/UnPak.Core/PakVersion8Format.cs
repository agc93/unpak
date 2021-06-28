using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnPak.Core.Crypto;
using static UnPak.Core.FormatHelpers;

namespace UnPak.Core
{
    public class PakVersion8Format : IPakFormat
    {
        private readonly IHashProvider _hashProvider;
        public SupportedOperations Supports => SupportedOperations.Read;
        public IEnumerable<int> Versions => new[] {8};
        public uint? Magic => null;

        public PakVersion8Format(IHashProvider hashProvider) {
            _hashProvider = hashProvider;
        }
        public Record ReadRecord(BinaryReader binaryReader, string fileName) {
            var offset = binaryReader.ReadUInt64();
            var compressedSize = binaryReader.ReadUInt64();
            var rawSize = binaryReader.ReadUInt64();
            var compressionVal = binaryReader.ReadUInt32();
            var compression = Enum.Parse<CompressionMethod>(compressionVal.ToString());
            var hash = binaryReader.ReadBytes(20);
            var blocks = GetBlocks(compression, binaryReader);
            var encrypted = binaryReader.ReadChar();
            var blockSize = binaryReader.ReadUInt32();
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

        public byte[] WriteRecord(BinaryWriter binaryWriter, ArchiveFile file, bool isEncrypted, PackageCompression? compression) {
            throw new NotImplementedException();
        }
    }
}
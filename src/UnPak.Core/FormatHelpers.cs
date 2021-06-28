using System;
using System.Collections.Generic;
using System.IO;

namespace UnPak.Core
{
    public static class FormatHelpers
    {
        public static IEnumerable<CompressionBlock> GetBlocks(CompressionMethod method, BinaryReader streamReader) {
            switch (method) {
                case CompressionMethod.None:
                    yield break;
                case CompressionMethod.Zlib:
                    var blockCount = streamReader.ReadUInt32();
                    for (var i = 0; i < blockCount; i++) {
                        yield return new CompressionBlock {
                            StartOffset = streamReader.ReadUInt64(),
                            EndOffset = streamReader.ReadUInt64()
                        };
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(method), method, null);
            }
        }
    }
}
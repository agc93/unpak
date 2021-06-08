using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace UnPak.Core
{
    public static class CoreExtensions
    {
        public static IPakFormat? GetFormat(this IEnumerable<IPakFormat> pakFormats, SupportedOperations operation, FileFooter footer, PakLayoutOptions opts = null) {
            return pakFormats.FirstOrDefault(f =>
                f.Versions.Any(v=> v == footer.Version) && (f.Magic ?? opts.Magic) == footer.Magic && f.Supports.HasFlag(operation));
        }
        
        public static IPakFormat? GetFormat(this IEnumerable<IPakFormat> pakFormats, SupportedOperations operation, int version) {
            
            return pakFormats.FirstOrDefault(f => f.Versions.Any(v => v== version) && f.Supports.HasFlag(operation));
        }

        public static IEnumerable<CompressionBlock> OffsetBy(this IEnumerable<CompressionBlock> blocks, ulong offset) {
            return blocks.Select(block => new CompressionBlock {
                StartOffset = block.StartOffset + offset,
                EndOffset = block.EndOffset + offset
            });
        }
    }
}
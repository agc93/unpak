using System.Collections.Generic;
using System.Linq;

namespace UnPak.Core
{
    public static class CoreExtensions
    {
        public static IPakFormat? GetFormat(this IEnumerable<IPakFormat> pakFormats, FileFooter footer, PakLayoutOptions opts = null) {
            
            return pakFormats.FirstOrDefault(f =>
                f.Version == footer.Version && (opts?.Magic == null || (f.Magic ?? opts.Magic) == footer.Magic));
        }
        
        public static IPakFormat? GetFormat(this IEnumerable<IPakFormat> pakFormats, int version) {
            
            return pakFormats.FirstOrDefault(f => f.Version == version);
        }
    }
}
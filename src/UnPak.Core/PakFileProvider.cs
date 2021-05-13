using System.Collections.Generic;
using System.IO;

namespace UnPak.Core
{
    public class PakFileProvider
    {
        private IEnumerable<IPakFormat> _formats;
        private PakLayoutOptions? _opts;

        public PakFileProvider(IEnumerable<IPakFormat> pakFormats, PakLayoutOptions? opts) {
            _formats = pakFormats;
            _opts = opts;
        }

        public PakFileReader GetReader(FileStream fileStream, PakLayoutOptions opts = null) {
            return new(fileStream, _formats, opts ?? _opts);
        }
        
        public PakFileReader GetReader(FileInfo fileInfo, PakLayoutOptions opts = null) {
            return new(fileInfo.OpenRead(), _formats, opts ?? _opts);
        }
        
        public PakFileWriter GetWriter(PakLayoutOptions opts = null) {
            return new PakFileWriter(_formats, opts);
        }
    }
}
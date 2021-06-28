using System;
using System.Collections.Generic;
using System.IO;
using UnPak.Core.Crypto;

namespace UnPak.Core
{
    public class PakFileProvider
    {
        private IEnumerable<IPakFormat> _formats;
        private readonly IHashProvider? _hashProvider;
        private readonly IEnumerable<IFooterLayout> _footerLayouts;
        private PakLayoutOptions? _opts;

        public PakFileProvider(IEnumerable<IPakFormat> pakFormats, IEnumerable<IFooterLayout> footerLayouts, IHashProvider hashProvider, PakLayoutOptions opts) : this(pakFormats, footerLayouts, hashProvider) {
            _opts = opts;
        }
        
        public PakFileProvider(IEnumerable<IPakFormat> pakFormats, IEnumerable<IFooterLayout> footerLayouts, IHashProvider hashProvider) : this(pakFormats) {
            _hashProvider = hashProvider;
            _footerLayouts = footerLayouts;
        }

        public PakFileProvider(IEnumerable<IPakFormat> pakFormats) {
            _formats = pakFormats;
        }

        public PakFileReader GetReader(FileStream fileStream, PakLayoutOptions opts = null) {
            return new(fileStream, _formats, _footerLayouts, opts ?? _opts);
        }
        
        public PakFileReader GetReader(FileInfo fileInfo, PakLayoutOptions opts = null) {
            return new(fileInfo.OpenRead(), _formats, _footerLayouts, opts ?? _opts);
        }
        
        public PakFileWriter GetWriter(PakLayoutOptions opts = null) {
            return new PakFileWriter(_formats, _hashProvider ?? new ManagedHashProvider(), opts ?? _opts);
        }
    }
}
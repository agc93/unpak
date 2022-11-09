using System.Collections.Generic;
using System.IO;
using UnPak.Core.Crypto;

namespace UnPak.Core
{
    public class PakFileProvider
    {
        private readonly IEnumerable<IPakFormat> _formats;
        private readonly IHashProvider? _hashProvider;
        private readonly IEnumerable<IFooterLayout> _footerLayouts;
        private readonly PakLayoutOptions? _opts;

        public PakFileProvider(IEnumerable<IPakFormat> pakFormats, IEnumerable<IFooterLayout> footerLayouts, IHashProvider hashProvider, PakLayoutOptions opts) : this(pakFormats, footerLayouts, hashProvider) {
            _opts = opts;
        }
        
        public PakFileProvider(IEnumerable<IPakFormat> pakFormats, IEnumerable<IFooterLayout> footerLayouts, IHashProvider hashProvider) : this(pakFormats, footerLayouts) {
            _hashProvider = hashProvider;
        }

        private PakFileProvider(IEnumerable<IPakFormat> pakFormats, IEnumerable<IFooterLayout> footerLayouts) {
            _footerLayouts = footerLayouts;
            _formats = pakFormats;
        }

        public PakFileReader GetReader(FileStream fileStream, PakLayoutOptions? opts = null) {
            return new(fileStream, _formats, _footerLayouts, opts ?? _opts);
        }
        
        public PakFileReader GetReader(FileInfo fileInfo, PakLayoutOptions? opts = null) {
            return new(fileInfo.OpenRead(), _formats, _footerLayouts, opts ?? _opts);
        }
        
        public PakFileWriter GetWriter(PakLayoutOptions? opts = null) {
            return new PakFileWriter(_formats, _hashProvider ?? new ManagedHashProvider(), opts ?? _opts);
        }
        
        public static PakFileProvider GetDefaultProvider() {
            var crypto = new ManagedHashProvider();
            var provider =
                new PakFileProvider(
                    new IPakFormat[]
                        { new PakVersion3Format(crypto), new PakVersion4Format(), new PakVersion8Format(crypto) },
                    new IFooterLayout[] { new DefaultFooterLayout(), new PaddedFooterLayout() }, crypto);
            return provider;
        }
    }
}
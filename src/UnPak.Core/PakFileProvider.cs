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
        private PakLayoutOptions? _opts;

        public PakFileProvider(IEnumerable<IPakFormat> pakFormats, IHashProvider hashProvider, PakLayoutOptions opts) : this(pakFormats, hashProvider) {
            
            _opts = opts;
        }
        
        public PakFileProvider(IEnumerable<IPakFormat> pakFormats, IHashProvider hashProvider) : this(pakFormats) {
            _hashProvider = hashProvider;
        }

        public PakFileProvider(IEnumerable<IPakFormat> pakFormats) {
            _formats = pakFormats;
        }

        public PakFileReader GetReader(FileStream fileStream, PakLayoutOptions opts = null) {
            return new(fileStream, _formats, opts ?? _opts);
        }
        
        public PakFileReader GetReader(FileInfo fileInfo, PakLayoutOptions opts = null) {
            return new(fileInfo.OpenRead(), _formats, opts ?? _opts);
        }
        
        public PakFileWriter GetWriter(PakLayoutOptions opts = null) {
            return new PakFileWriter(_formats, _hashProvider ?? new ManagedHashProvider(), opts ?? _opts);
        }
    }
}
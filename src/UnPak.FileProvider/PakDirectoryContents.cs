using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.FileProviders;
using UnPak.Core;

namespace UnPak.FileProvider
{
    // This is never used by BlazorWebView or WebViewManager
    internal sealed class PakDirectoryContents : IDirectoryContents
    {

        private readonly PakFile _pakFile;
        private readonly string _directoryPath;
        private readonly IEnumerable<Record> _records;

        public PakDirectoryContents(PakFile pakFile, string directoryPath)
        {
            _pakFile = pakFile;
            _directoryPath = directoryPath.Replace('\\', '/');
            _records = pakFile.Where(r => r.FileName.StartsWith(_directoryPath));
            Exists = _records.Any();
        }

        public bool Exists { get; set; }

        public IEnumerator<IFileInfo> GetEnumerator()
            => _records.Select(r => new PakFileInfo(_pakFile, r)).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => _records.Select(r => new PakFileInfo(_pakFile, r)).GetEnumerator();
    }
}
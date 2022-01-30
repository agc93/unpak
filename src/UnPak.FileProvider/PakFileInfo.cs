using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.FileProviders;
using UnPak.Core;

namespace UnPak.FileProvider
{
    internal sealed class PakFileInfo : IFileInfo
    {
        private readonly PakFile _pakFile;
        private readonly Record? _record;

        public PakFileInfo(PakFile pakFile, string filePath)
        {
            _pakFile = pakFile;
            var filePath1 = filePath.Replace('\\', '/');
            _record = pakFile.FirstOrDefault(r => r.FileName.Contains(filePath1));
            Exists = _record != null;
            Length = Exists ? Convert.ToInt64(_record!.RawSize) : -1;
            Name = Path.GetFileName(_record?.FileName ?? filePath);
        }

        internal PakFileInfo(PakFile pakFile, Record record) {
            _pakFile = pakFile;
            _record = record;
            Exists = _record != null;
            Length = Exists ? Convert.ToInt64(_record!.RawSize) : -1;
            Name = Path.GetFileName(record.FileName);
        }

        public bool Exists { get; }
        public long Length { get; }
        public string PhysicalPath { get; } = null!;
        public string Name { get; }
        public DateTimeOffset LastModified { get; internal set; } = DateTimeOffset.FromUnixTimeSeconds(0);
        public bool IsDirectory => false;

        public Stream CreateReadStream()
        {
            return _record == null ? throw new InvalidOperationException() : _record.Unpack(_pakFile.FileStream);
        }
    }
}
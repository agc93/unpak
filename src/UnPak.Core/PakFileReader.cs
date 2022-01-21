using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnPak.Core.Diagnostics;

namespace UnPak.Core
{
    public class PakFileReader : IDisposable
    {
        private readonly IEnumerable<IPakFormat> _formats;
        private readonly IEnumerable<IFooterLayout> _footerFormats;
        // ReSharper disable InconsistentNaming
        private BinaryReader _reader { get; }
        private FileStream _stream { get; }
        private PakLayoutOptions _layout { get; }
        // ReSharper restore InconsistentNaming

        public PakFileReader(FileStream fileStream, IEnumerable<IPakFormat> pakFormats, IEnumerable<IFooterLayout> footerFormats, PakLayoutOptions? opts) {
            _stream = fileStream;
            _reader = new BinaryReader(_stream, Encoding.ASCII);
            _layout = opts ?? new PakLayoutOptions();
            _formats = pakFormats;
            _footerFormats = footerFormats;
        }

        public PakFile ReadFile() {
            var footer = ReadFooter();
            if (footer == null) {
                throw new InvalidDataException("Could not read footer layout!");
            }
            _stream.Seek(footer.IndexOffset, SeekOrigin.Begin);
            var mountPoint = _reader.ReadUEString(true);
            var entryCount = _reader.ReadUInt32();
            if (mountPoint == null) {
                throw new InvalidOperationException("Could not determine mount point from input file!");
            }
            var format = _formats.GetFormat(SupportedOperations.Read, footer, _layout);
            if (format == null) {
                throw new FormatNotSupportedException(footer.Version);
            }
            var pak = new PakFile(mountPoint, footer, _stream);
            for (int i = 0; i < entryCount; i++) {
                //TODO: this is a bad idea tbh
                var fileName = _reader.ReadPath() ?? _reader.ReadString();
                var record = format.ReadRecord(_reader, fileName);
                pak.Records.Add(record);
            }

            if (_stream.Position > footer.FooterOffset) {
                // this doesn't feel like it should be possible, but u4pak handles it so here we are
                throw new FileStructureException("Index is too long and collides with footer!");
            }
            return pak;
        }

        public FileFooter? ReadFooter() {
            return _footerFormats.Select(footerFormat => footerFormat.ReadFooter(_reader, _layout))
                .FirstOrDefault(footer => footer != null && footer.Version != 0);
        }

        public List<FileInfo> UnpackTo(DirectoryInfo targetPath, PakFile? file = null) {
            file ??= ReadFile();
            var unpack = file.UnpackAll(_stream, targetPath).ToList();
            return unpack;
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                _reader.Dispose();
                _stream.Dispose();
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}

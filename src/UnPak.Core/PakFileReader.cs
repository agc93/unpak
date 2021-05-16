using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UnPak.Core
{
    public class PakFileReader : IDisposable
    {
        private IEnumerable<IPakFormat> _formats;
        private BinaryReader _reader { get; }
        private FileStream _stream { get; }
        private PakLayoutOptions _layout { get; } = new PakLayoutOptions();

        public FileStream BackingFile => _stream;
        

        public PakFileReader(FileStream fileStream, IEnumerable<IPakFormat> pakFormats, PakLayoutOptions opts) {
            _stream = fileStream;
            _reader = new BinaryReader(_stream, Encoding.ASCII);
            _layout = opts ?? new PakLayoutOptions();
            _formats = pakFormats;
        }

        public PakFile ReadFile() {
            var footer = ReadFooter();
            _stream.Seek(footer.IndexOffset, SeekOrigin.Begin);
            var mountPoint = _reader.ReadUEString(true);
            var entryCount = _reader.ReadUInt32();
            var format = GetFormat(footer);
            if (format == null) {
                throw new InvalidOperationException($"No matching format for version '{footer.Version}' found!");
            }
            var pak = new PakFile(mountPoint, footer);
            for (int i = 0; i < entryCount; i++) {
                var fileName = _reader.ReadPath();
                var record = format.ReadRecord(_reader, fileName);
                pak.Records.Add(record);
            }

            if (_stream.Position > footer.FooterOffset) {
                // this doesn't feel like it should be possible, but u4pak handles it so here we are
                throw new InvalidDataException("Index is too long and collides with footer!");
            }
            return pak;
        }

        public List<FileInfo> UnpackTo(DirectoryInfo targetPath, PakFile file = null) {
            file ??= ReadFile();
            var unpack = file.UnpackAll(_stream, targetPath).ToList();
            return unpack;
        }

        private IPakFormat GetFormat(FileFooter footer) {
            return _formats.FirstOrDefault(f =>
                f.Version == footer.Version && (f.Magic ?? _layout.Magic) == footer.Magic);
        }

        public FileFooter ReadFooter() {
            var curr = _stream.Position;
            _stream.Seek(-_layout.FooterLength, SeekOrigin.End);
            var footerOffset = _stream.Position;
            var magic = _reader.ReadUInt32();
            var version = _reader.ReadUInt32();
            var indexOffset = _reader.ReadUInt64();
            var indexSize = _reader.ReadUInt64();
            var hash = _reader.ReadChars(_layout.HashLength);
            _stream.Seek(curr, SeekOrigin.Begin);
            return new FileFooter {
                Magic = magic,
                Version = version,
                IndexOffset = (long) indexOffset,
                IndexLength = indexSize,
                FooterOffset = footerOffset,
                IndexHash = new string(hash)
            };
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                _reader?.Dispose();
                _stream?.Dispose();
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}

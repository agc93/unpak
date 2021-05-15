using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace UnPak.Core
{
    public class PakVersion3Format : IPakFormat
    {
        public int Version => 3;
        public uint? Magic => null;

        public Record ReadRecord(BinaryReader streamReader, string fileName) {
            var offset = streamReader.ReadUInt64();
            var compressedSize = streamReader.ReadUInt64();
            var rawSize = streamReader.ReadUInt64();
            var compressionVal = streamReader.ReadUInt32();
            var compression = Enum.Parse<CompressionMethod>(compressionVal.ToString());
            var hash = streamReader.ReadBytes(20);
            var blocks = GetBlocks(compression, streamReader);
            var encrypted = streamReader.ReadChar();
            var blockSize = streamReader.ReadUInt32();
            return new Record(53) {
                FileName = fileName,
                RecordOffset = offset,
                CompressedSize = compressedSize,
                RawSize = rawSize,
                CompressionMethod = compression,
                Hash = hash,
                Encrypted = encrypted != 0,
                CompressionBlockSize = blockSize
            };
        }

        public Record WriteCompressedRecord(BinaryWriter binaryWriter, FileInfo file, bool isEncrypted, CompressionMethod compression,
            uint compressionBlockSize) {
            throw new NotImplementedException();
        }

        public byte[] WriteRecord(BinaryWriter binaryWriter, ArchiveFile archiveFile, bool isEncrypted) {
            var file = archiveFile.File;
            var curr = binaryWriter.BaseStream.Position;
            var size = file.Length;
            var recordBytes = new byte[53 + file.Length];
            var hash = GetSha1Hash(file);
            using var tgtStream = new MemoryStream((int) (53 + file.Length));
            using var writer = new BinaryWriter(tgtStream, Encoding.ASCII, true);
            var header = GetIndexRecord(0, (file.Length, file.Length), hash, CompressionMethod.None, isEncrypted, 0);
            writer.Write(header);
            writer.WriteFile(file);
            writer.Flush();
            var dataRecord = tgtStream.ToArray();
            binaryWriter.Write(dataRecord);
            
            var indexRecord = GetIndexRecord(curr, (file.Length, file.Length), hash, CompressionMethod.None, isEncrypted, 0);
            return indexRecord;
        }

        private byte[] GetIndexRecord(long offset, (long compressedLength, long rawLength) size, byte[] hash, CompressionMethod compressionMethod,
            bool isEncrypted, uint compressionBlockSize) {
            using var tgtStream = new MemoryStream();
            using var writer = new BinaryWriter(tgtStream, Encoding.ASCII);
            writer.WriteUInt64((ulong) offset);
            writer.WriteInt64(size.compressedLength);
            writer.WriteInt64(size.rawLength);
            writer.WriteUInt32((uint) compressionMethod); // compression method
            writer.Write(hash);
            writer.Write((byte)(isEncrypted ? 0x01 : 0x00)); //encryption
            writer.WriteUInt32(compressionBlockSize); //compression block size
            writer.Flush();
            return tgtStream.ToArray();
        }

        private byte[] GetSha1Hash(FileInfo fi, Encoding encoding = null) {
            encoding ??= Encoding.ASCII;
            using var fs = fi.OpenRead();
            using var sha1 = new SHA1Managed();
            var hash = sha1.ComputeHash(fs);
            // return encoding.GetString()
            return hash;
        }

        private Dictionary<int, int> GetBlocks(CompressionMethod method, BinaryReader streamReader) {
            switch (method) {
                case CompressionMethod.None:
                    return new Dictionary<int, int>();
                case CompressionMethod.Zlib:
                    var blockCount = streamReader.ReadUInt32();
                    throw new NotImplementedException();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(method), method, null);
            }
        }
    }

    public enum CompressionMethod
    {
        None = 0,
        Zlib = 1
    }

    public interface IPakFormat
    {
        public int Version { get; }
        public uint? Magic { get; }
        public Record ReadRecord(BinaryReader binaryReader, string fileName);
        public byte[] WriteRecord(BinaryWriter binaryWriter, ArchiveFile file, bool isEncrypted);
        public Record WriteCompressedRecord(BinaryWriter binaryWriter, FileInfo file, bool isEncrypted, CompressionMethod compression, uint compressionBlockSize);
    }

    public record Block {}

    public class PakFile : IEnumerable<Record>
    {
        public PakFile(string mountPoint, FileFooter fileFooter) {
            MountPoint = mountPoint;
            FileFooter = fileFooter;
        }

        public FileFooter FileFooter { get; protected set; }

        public string MountPoint { get; protected set; }
        public List<Record> Records { get; } = new List<Record>();
        public IEnumerator<Record> GetEnumerator() {
            return Records.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return ((IEnumerable) Records).GetEnumerator();
        }

        public IEnumerable<FileInfo> UnpackAll(FileStream pakFile, DirectoryInfo unpackRoot) {
            return Records.Select(fileRecord => fileRecord.Unpack(pakFile, unpackRoot));
        }
    }

    public class PakFileWriter
    {
        private readonly IEnumerable<IPakFormat> _formats;
        private PakLayoutOptions _opts;

        public PakFileWriter(IEnumerable<IPakFormat> pakFormats, PakLayoutOptions opts) {
            _formats = pakFormats;
            _opts = opts ?? new PakLayoutOptions();
        }

        public FileInfo BuildFromDirectory(DirectoryInfo srcPath, FileInfo outputFile, PakFileCreationOptions opts = null) {
            opts ??= new PakFileCreationOptions();
            using var outputStream = outputFile.OpenWrite();
            var files = srcPath.EnumerateFiles("*", SearchOption.AllDirectories).ToList();
            files = files.OrderBy(f => f.Name).ToList();
            var inputFiles = files.ToDictionary(f => Path.GetRelativePath(srcPath.Parent.FullName, f.FullName), f => f);
            var records = WriteDataFiles(inputFiles, outputStream, opts);
            WriteIndex(outputStream, records, opts);
            return new FileInfo(outputFile.FullName);
        }

        public void BuildFromFiles(Dictionary<string, FileInfo> files, FileInfo outputFile,
            PakFileCreationOptions opts = null) {
            opts ??= new PakFileCreationOptions();
            using var outputStream = outputFile.OpenWrite();
        }

        public Dictionary<string, byte[]> WriteDataFiles(Dictionary<string, FileInfo> srcFiles, FileStream outputStream, PakFileCreationOptions opts) {
            var format = _formats.GetFormat(opts.ArchiveVersion);
            
            var records = new Dictionary<string, byte[]>();
            using var writer = new BinaryWriter(outputStream, Encoding.ASCII, true);
            foreach (var (relPath, fileInfo) in srcFiles) {
                var file = new ArchiveFile { File = fileInfo, Path = relPath };
                var record = format.WriteRecord(writer, file, false);
                records.Add(relPath, record);
            }

            return records;
        }

        private void WriteIndex(Stream outWriter, Dictionary<string, byte[]> records, PakFileCreationOptions opts) {
            var indexOffset = outWriter.Position;
            using var sha1 = new SHA1Managed();
            using var indexStream = new MemoryStream();
            var mountPoint = opts.MountPoint.EncodePath();
            var recordLength = BitConverter.GetBytes(records.Count);
            var indexHeader = mountPoint.Concat(recordLength).ToArray();
            //var indexSize = indexHeader.LongLength;
            indexStream.Write(indexHeader);
            foreach (var (path, indexRecord) in records) {
                var pathBytes = path.EncodePath();
                indexStream.Write(pathBytes);
                indexStream.Write(indexRecord);
            }
            var indexHash = sha1.ComputeHash(indexStream);
            using var footerStream = new MemoryStream();
            using var writer = new BinaryWriter(footerStream, Encoding.UTF8, true);
            writer.WriteUInt32(opts.Magic);
            writer.WriteUInt32((uint) opts.ArchiveVersion);
            writer.WriteUInt64((ulong) indexOffset);
            writer.WriteUInt64((ulong) indexStream.Length);
            writer.Write(indexHash);
            writer.Close();
            
            outWriter.Write(indexStream.ToArray());
            outWriter.Write(footerStream.ToArray());
        }
    }

    public record ArchiveFile
    {
        public FileInfo File { get; init; }
        public string Path { get; init; }
    }

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

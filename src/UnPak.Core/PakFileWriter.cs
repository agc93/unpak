using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnPak.Core.Crypto;
using UnPak.Core.Diagnostics;

namespace UnPak.Core
{
    public class PakFileWriter
    {
        private readonly IEnumerable<IPakFormat> _formats;
        private readonly IHashProvider _hashProvider;
        private PakLayoutOptions _opts;

        public PakFileWriter(IEnumerable<IPakFormat> pakFormats, IHashProvider hashProvider, PakLayoutOptions? opts) {
            _formats = pakFormats;
            _hashProvider = hashProvider;
            _opts = opts ?? new PakLayoutOptions();
        }

        public FileInfo BuildFromDirectory(DirectoryInfo srcPath, FileInfo outputFile, PakFileCreationOptions? opts = null) {
            opts ??= new PakFileCreationOptions();
            var files = srcPath.EnumerateFiles("*", SearchOption.AllDirectories).ToList();
            files = files.OrderBy(f => f.Name).ToList();
            var inputFiles = files.ToDictionary(f => Path.GetRelativePath(srcPath.Parent?.FullName ?? srcPath.FullName, f.FullName), f => f);
            using var outputStream =
                new FileStream(outputFile.FullName, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
            var records = WriteDataFiles(inputFiles, outputStream, opts);
            WriteIndex(outputStream, records, opts);
            return new FileInfo(outputFile.FullName);
        }

        private FileInfo BuildFromFiles(Dictionary<string, FileInfo> files, FileInfo outputFile,
            PakFileCreationOptions? opts = null) {
            opts ??= new PakFileCreationOptions();
            using var outputStream =
                new FileStream(outputFile.FullName, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
            var records = WriteDataFiles(files, outputStream, opts);
            WriteIndex(outputStream, records, opts);
            return new FileInfo(outputFile.FullName);
        }

        public Dictionary<string, byte[]> WriteDataFiles(Dictionary<string, FileInfo> srcFiles, FileStream outputStream, PakFileCreationOptions opts) {
            var format = _formats.GetFormat(SupportedOperations.Write, opts.ArchiveVersion);
            if (format == null) {
                throw new FormatNotSupportedException(opts.ArchiveVersion);
            }
            
            var records = new Dictionary<string, byte[]>();
            using var writer = new BinaryWriter(outputStream, Encoding.ASCII, true);
            foreach (var (relPath, fileInfo) in srcFiles) {
                var file = new ArchiveFile { File = fileInfo, Path = relPath };
                var record = format.WriteRecord(writer, file, false, opts?.Compression);
                records.Add(relPath, record);
            }

            return records;
        }

        private void WriteIndex(Stream outWriter, Dictionary<string, byte[]> records, PakFileCreationOptions opts) {
            var indexOffset = outWriter.Position;
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
            var fileIndex = indexStream.ToArray();
            var indexHash = _hashProvider.GetSha1Hash(fileIndex);
            using var footerStream = new MemoryStream();
            using var writer = new BinaryWriter(footerStream, Encoding.UTF8, true);
            writer.WriteUInt32(opts.Magic);
            writer.WriteUInt32((uint) opts.ArchiveVersion);
            writer.WriteUInt64((ulong) indexOffset);
            writer.WriteUInt64((ulong) fileIndex.Length);
            writer.Write(indexHash);
            writer.Close();
            
            outWriter.Write(fileIndex);
            outWriter.Write(footerStream.ToArray());
        }
    }
}
using System;
using System.IO;

namespace UnPak.Core
{
    public record ArchiveFile
    {
        public ArchiveFile(string path) {
            Path = path;
        }
        public FileInfo? File { get; init; }
        public string Path { get; init; }
        public Stream? RawData { get; init; }

        public Stream GetData() {
            if (File is not null) {
                return File.OpenRead();
            } else if (RawData is not null) {
                return RawData;
            }

            throw new InvalidOperationException("No archive file data present!");
        }
    }
}
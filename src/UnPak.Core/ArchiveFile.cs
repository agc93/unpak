using System.IO;

namespace UnPak.Core
{
    public record ArchiveFile
    {
        public FileInfo File { get; init; }
        public string Path { get; init; }
    }
}
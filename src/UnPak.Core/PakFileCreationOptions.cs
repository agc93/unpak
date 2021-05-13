using System.IO;

namespace UnPak.Core
{
    public record PakFileCreationOptions
    {
        public int ArchiveVersion { get; init; } = 3;
        public string MountPoint { get; init; } = Path.Join("..", "..", "..", "\\");
        public uint Magic { get; init; } = 0x5A6F12E1;
    }
}
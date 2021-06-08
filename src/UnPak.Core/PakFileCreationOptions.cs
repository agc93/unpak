using System.IO;

namespace UnPak.Core
{
    public record PakFileCreationOptions
    {
        public PakFileCreationOptions() {
            
        }

        public PakFileCreationOptions(int? archiveVersion, string? mountPoint, uint? magic) {
            if (archiveVersion != null) ArchiveVersion = archiveVersion.Value;
            if (mountPoint != null) MountPoint = mountPoint;
            if (magic != null) Magic = magic.Value;
        }
        public int ArchiveVersion { get; init; } = 3;
        public string MountPoint { get; init; } = Path.Join("..", "..", "..", "\\");
        public uint Magic { get; init; } = 0x5A6F12E1;
        public PackageCompression? Compression { get; init; }
    }
}
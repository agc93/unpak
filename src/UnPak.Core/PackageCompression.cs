namespace UnPak.Core
{
    public class PackageCompression
    {
        public PackageCompression(CompressionMethod method) {
            Method = method;
        }
        public CompressionMethod Method { get; }
        public uint BlockSize { get; init; } = 65536;
    }
}
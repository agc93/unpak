namespace UnPak.Core
{
    public record CompressionBlock
    {
        public ulong StartOffset { get; init; }
        public ulong EndOffset { get; init; }
        public ulong BlockSize => EndOffset - StartOffset;
    }
}
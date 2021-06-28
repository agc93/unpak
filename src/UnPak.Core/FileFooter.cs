namespace UnPak.Core
{
    public record FileFooter
    {
        public long FooterOffset { get; init; }
        public uint Magic { get; init; }
        public uint Version { get; init; }
        public long IndexOffset { get; init; }
        public ulong IndexLength { get; init; }
        public string IndexHash { get; init; }
        public byte[] RawIndexHash { get; init; }
    }
}
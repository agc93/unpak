namespace UnPak.Core
{
    public record PakLayoutOptions
    {
        public int HashLength { get; init; } = 20;
        public int FooterLength { get; init; } = 44;
        public uint Magic { get; init; } = 0x5A6F12E1;
    }
}
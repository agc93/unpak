using System.IO;

namespace UnPak.Core
{
    public interface IFooterLayout
    {
        int FooterLength { get; }
        FileFooter? ReadFooter(BinaryReader reader, PakLayoutOptions? options);
    }
}
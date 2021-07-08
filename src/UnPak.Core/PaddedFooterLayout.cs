using System;
using System.IO;
using System.Linq;

namespace UnPak.Core
{
    public class PaddedFooterLayout : IFooterLayout
    {
        public int FooterLength => 44;
        private int HashLength => 20;
        public FileFooter ReadFooter(BinaryReader reader, PakLayoutOptions? options) {
            var curr = reader.BaseStream.Position;
            reader.BaseStream.Seek(-148, SeekOrigin.End);
            var temphash = reader.ReadChars(20);
            if (temphash.All(hc => hc != 0x0)) {
                //128-bit padded
                reader.BaseStream.Seek(-(128 + HashLength) , SeekOrigin.End);
            } else {
                reader.BaseStream.Seek(-(160 + HashLength), SeekOrigin.End);
                temphash = reader.ReadChars(20);
                if (temphash.Any(hc => hc == 0x0)) return null;
                reader.BaseStream.Seek(-(160 + FooterLength), SeekOrigin.End);
            }
            var footerOffset = reader.BaseStream.Position;
            var magic = reader.ReadUInt32();
            var version = reader.ReadUInt32();
            var indexOffset = reader.ReadUInt64();
            var indexSize = reader.ReadUInt64();
            var hash = reader.ReadBytes(20);
            reader.BaseStream.Seek(curr, SeekOrigin.Begin);
            return new FileFooter {
                Magic = magic,
                Version = version,
                IndexOffset = (long) indexOffset,
                IndexLength = indexSize,
                FooterOffset = footerOffset,
                IndexHash = BitConverter.ToString(hash),
                RawIndexHash = hash
            };
        }
    }
}
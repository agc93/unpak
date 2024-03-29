﻿using System;
using System.IO;
using System.Linq;

namespace UnPak.Core
{
    public class DefaultFooterLayout : IFooterLayout
    {
        public int FooterLength => 44;
        public FileFooter? ReadFooter(BinaryReader reader, PakLayoutOptions? options) {
            var curr = reader.BaseStream.Position;
            reader.BaseStream.Seek(-20, SeekOrigin.End);
            var tempHash = reader.ReadChars(20);
            if (tempHash.Count(c => c == 0x0) > 4) {
                //this (probably) ain't it chief
                return null;
            }
            /*if (temphash.Any(hc => hc == 0x0)) {
                //this ain't it chief
                return null;
            }*/
            reader.BaseStream.Seek(-FooterLength, SeekOrigin.End);
            var footerOffset = reader.BaseStream.Position;
            var magic = reader.ReadUInt32();
            var version = reader.ReadUInt32();
            var indexOffset = reader.ReadUInt64();
            var indexSize = reader.ReadUInt64();
            var hash = reader.ReadBytes(20);
            if (magic != 1517228769 || version > 15 || indexOffset > (ulong) reader.BaseStream.Length) {
                return null;
            }
            //TODO: need to actually sanity check all these wild reads
            // it could be valid data but semantically nonsense
            // or (more likely) it could just be empty
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
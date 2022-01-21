using System;
using System.IO;
using System.Linq;
using System.Text;

namespace UnPak.Core
{
    public static class BinaryReaderExtensions
    {
        private static readonly Encoding Utf8 = new UTF8Encoding(false);

        public static string? ReadPath(this BinaryReader reader) {
            return reader.ReadUEString(true);
        }

        public static string? ReadUEString(this BinaryReader reader, bool trimEnd = false)
        {
            if (reader.PeekChar() < 0)
                return null;

            var length = reader.ReadInt32();
            if (length == 0)
                return null;

            if (length == 1)
                return "";

            var valueBytes = reader.ReadBytes(length);

            return trimEnd
                ? Utf8.GetString(valueBytes, 0, valueBytes.Length - 1).TrimEnd(new[] {(char) 0})
                : Utf8.GetString(valueBytes, 0, valueBytes.Length - 1);
        }
    }
    
    public static class BinaryWriterExtensions
    {
        public static void WriteInt64(this BinaryWriter writer, long value)
        {
            writer.Write(BitConverter.GetBytes(value));
        }
        
        public static void WriteUInt64(this BinaryWriter writer, ulong value)
        {
            writer.Write(BitConverter.GetBytes(value));
        }

        public static void WriteInt32(this BinaryWriter writer, int value)
        {
            writer.Write(BitConverter.GetBytes(value));
        }
        
        public static void WriteUInt32(this BinaryWriter writer, uint value)
        {
            writer.Write(BitConverter.GetBytes(value));
        }

        public static void WriteInt16(this BinaryWriter writer, short value)
        {
            writer.Write(BitConverter.GetBytes(value));
        }
        public static void WriteUInt16(this BinaryWriter writer, ushort value)
        {
            writer.Write(BitConverter.GetBytes(value));
        }
        public static void WriteSingle(this BinaryWriter writer, float value)
        {
            writer.Write(BitConverter.GetBytes(value));
        }

        public static void WriteString(this BinaryWriter writer, string value, Encoding? encoding = null) {
            encoding ??= Encoding.ASCII;
            writer.Write(encoding.GetBytes(value));
        }

        public static void WriteFile(this BinaryWriter writer, FileInfo fi) {
            /*var bytes = File.ReadAllBytes(fi.FullName);
            writer.Write(bytes);*/
            using var fs = fi.OpenRead();
            fs.CopyTo(writer.BaseStream);
        }
        
        public static BinaryWriter WritePath(this BinaryWriter writer, string path) {
            path = path.Replace(Path.PathSeparator, '/');
            var valueBytes = Encoding.UTF8.GetBytes(path);
            writer.Write(valueBytes.Length + 1);
            if (valueBytes.Length > 0)
                writer.Write(valueBytes);
            writer.Write((byte)0);
            return writer;
        }
        
        public static byte[] EncodePath(this string path, Encoding? encoding = null) {
            encoding ??= Encoding.UTF8;
            path = path.Replace(Path.DirectorySeparatorChar, '/');
            var valueBytes = encoding.GetBytes(path).Concat(new byte[]{0x00}).ToList();
            var lengthBytes = BitConverter.GetBytes(valueBytes.Count);
            return lengthBytes.Concat(valueBytes).ToArray();
        }
    }
}
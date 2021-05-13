using System;
using System.IO;
using System.Linq;
using System.Text;

namespace UnPak.Core
{
    public static class BinaryReaderExtensions
    {
        private static readonly Encoding Utf8 = new UTF8Encoding(false);

        public static string ReadPath(this BinaryReader reader) {
            return reader.ReadUEString(true);
        }

        public static string ReadUEString(this BinaryReader reader, bool trimEnd = false)
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
                ? Utf8.GetString(valueBytes, 0, valueBytes.Length - 1).TrimEnd(new char[] {(char) 0})
                : Utf8.GetString(valueBytes, 0, valueBytes.Length - 1);
        }

        [Obsolete("I don't know what this does, so please don't use it")]
        public static string ReadUEString(this BinaryReader reader, long vl)
        {
            if (reader.PeekChar() < 0)
                return null;

            var length = reader.ReadInt32();
            if (length == 0)
                return null;

            if (length == 1)
                return "";

            var valueBytes = reader.ReadBytes((int)vl - 4);
            return Utf8.GetString(valueBytes, 0, length - 1);
        }
    }
    
    public static class BinaryWriterExtensions
    {
        /*private static readonly Encoding Encoding = new ASCIIEncoding();
        
        public static void WriteUEString(this BinaryWriter writer, string value)
        {
            if (value == null)
            {
                writer.Write(0);
                return;
            }

            var valueBytes = Encoding.GetBytes(value);
            writer.Write(valueBytes.Length + 1);
            if (valueBytes.Length > 0)
                writer.Write(valueBytes);
            writer.Write((byte)0);
        }

        public static void WriteUEString(this BinaryWriter writer, string value, long vl)
        {
            if (value == null)
            {
                writer.Write(0);
                return;
            }

            var valueBytes = Encoding.GetBytes(value);
            writer.Write(valueBytes.Length + 1);
            if (valueBytes.Length > 0)
                writer.Write(valueBytes);
            writer.Write(false);
            while (vl > valueBytes.Length + 5)
            {
                writer.Write(false);
                vl--;
            }
        }*/

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

        public static void WriteString(this BinaryWriter writer, string value, Encoding encoding = null) {
            encoding ??= Encoding.ASCII;
            writer.Write(encoding.GetBytes(value));
        }

        public static void WriteFile(this BinaryWriter writer, FileInfo fi) {
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
        
        public static byte[] EncodePath(this string path, Encoding encoding = null) {
            encoding ??= Encoding.UTF8;
            path = path.Replace(Path.DirectorySeparatorChar, '/');
            var valueBytes = Encoding.UTF8.GetBytes(path).Concat(new byte[]{0x00});
            var lengthBytes = BitConverter.GetBytes(valueBytes.Count());
            return lengthBytes.Concat(valueBytes).ToArray();
        }
    }

    public static class StreamExtensions
    {
        public static void CopyStream(this Stream input, Stream output, long bytes)
        {
            byte[] buffer = new byte[32768];
            int read;
            while (bytes > 0 && 
                   (read = input.Read(buffer, 0, Math.Min(buffer.Length, (int)bytes))) > 0)
            {
                output.Write(buffer, 0, read);
                bytes -= read;
            }
        }
    }
}
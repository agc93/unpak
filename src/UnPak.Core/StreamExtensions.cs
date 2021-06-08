using System;
using System.IO;

namespace UnPak.Core
{
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
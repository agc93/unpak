using System;
using System.IO;

namespace UnPak.Core.Crypto
{
    public abstract class HashProvider : IHashProvider
    {
        public byte[] GetSha1Hash(FileInfo fi) {
            using var fs = fi.OpenRead();
            return GetSha1Hash(fs);
        }

        public abstract byte[] GetSha1Hash(Stream fs);
        public abstract byte[] GetSha1Hash(byte[] bytes);

        public string GetString(byte[] hash) {
            var chArrayLength = hash.Length * 2;
            var chArray = new char[chArrayLength];
            var i = 0;
            var index = 0;
            for (i = 0; i < chArrayLength; i += 2)
            {
                var b = hash[index++];
                chArray[i] = GetHexValue(b / 16);
                chArray[i + 1] = GetHexValue(b % 16);
            }
            return new string(chArray);
        }


        protected static char GetHexValue(int i)
        {
            if (i < 0 || i > 15)
            {
                throw new ArgumentOutOfRangeException("i", "i must be between 0 and 15.");
            }
            if (i < 10)
            {
                return (char)(i + '0');
            }
            return (char)(i - 10 + 'a');
        }
    }
}
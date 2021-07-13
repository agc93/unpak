using System.IO;
using System.Text;

namespace UnPak.Core.Crypto
{
    public interface IHashProvider
    {
        public byte[] GetSha1Hash(FileInfo fi);
        public byte[] GetSha1Hash(Stream fs);
        public byte[] GetSha1Hash(byte[] bytes);
        public string GetString(byte[] hash);
    }
}
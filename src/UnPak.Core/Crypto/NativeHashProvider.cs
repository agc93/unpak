using System.IO;
using System.Security.Cryptography;

namespace UnPak.Core.Crypto
{
    public class NativeHashProvider : HashProvider
    {
        public override byte[] GetSha1Hash(Stream fs) {
            using var hasher = SHA1.Create();
            var hash = hasher.ComputeHash(fs);
            return hash;
        }

        public override byte[] GetSha1Hash(byte[] bytes) {
            using var hasher = SHA1.Create();
            var hash = hasher.ComputeHash(bytes);
            return hash;
        }
    }
}
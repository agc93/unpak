using System.IO;
using System.Security.Cryptography;

namespace UnPak.Core.Crypto
{
    public class ManagedHashProvider : HashProvider
    {
        public override byte[] GetSha1Hash(Stream fs) {
            using var sha1 = new SHA1Managed();
            var hash = sha1.ComputeHash(fs);
            // return encoding.GetString()
            return hash;
        }

        public override byte[] GetSha1Hash(byte[] bytes) {
            using var sha1 = new SHA1Managed();
            var hash = sha1.ComputeHash(bytes);
            // return encoding.GetString()
            return hash;
        }
    }
}
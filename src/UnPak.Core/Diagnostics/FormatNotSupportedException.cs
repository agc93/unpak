using System;
using System.Runtime.Serialization;

namespace UnPak.Core.Diagnostics
{
    [Serializable]
    public class FormatNotSupportedException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public FormatNotSupportedException(int version) : base($"No matching format for version '{version}' found!") {
            this.Version = version;
        }

        public int Version { get; }

        protected FormatNotSupportedException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) {
        }

        public FormatNotSupportedException(uint version) : this((int)version) {
        }
    }
}
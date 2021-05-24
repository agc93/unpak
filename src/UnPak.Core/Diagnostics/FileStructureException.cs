using System;
using System.Runtime.Serialization;

namespace UnPak.Core.Diagnostics
{
    [Serializable]
    public class FileStructureException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public FileStructureException() {
        }

        public FileStructureException(string message) : base(message) {
        }

        public FileStructureException(string message, Exception inner) : base(message, inner) {
        }

        protected FileStructureException(
            SerializationInfo info,
            StreamingContext context) : base(info, context) {
        }
    }
}
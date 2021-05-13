using System.IO;
using System.Text;

namespace UnPak.Core
{
    public class RecordWriter : BinaryWriter
    {
        private Encoding _encoding;

        public RecordWriter(Stream tgtStream, Encoding encoding) : base(tgtStream, encoding) {
            _encoding = encoding;
        }
        
        public void WriteUEString(string value)
        {
            if (value == null)
            {
                Write(0);
                return;
            }

            var valueBytes = _encoding.GetBytes(value);
            Write(valueBytes.Length + 1);
            if (valueBytes.Length > 0) {
                Write(valueBytes);
            }
            Write((byte)0);
        }
        
        
    }
}
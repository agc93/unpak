using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UnPak.Core
{
    public class PakFile : IEnumerable<Record>
    {
        public PakFile(string mountPoint, FileFooter fileFooter) {
            MountPoint = mountPoint;
            FileFooter = fileFooter;
        }

        public FileFooter FileFooter { get; protected set; }

        public string MountPoint { get; protected set; }
        public List<Record> Records { get; } = new List<Record>();
        public IEnumerator<Record> GetEnumerator() {
            return Records.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return ((IEnumerable) Records).GetEnumerator();
        }

        public IEnumerable<FileInfo> UnpackAll(FileStream pakFile, DirectoryInfo unpackRoot) {
            return Records.Select(fileRecord => fileRecord.Unpack(pakFile, unpackRoot));
        }
    }
}
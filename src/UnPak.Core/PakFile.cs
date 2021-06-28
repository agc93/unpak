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

        public PakFile(string mountPoint, FileFooter fileFooter, Stream fileStream) : this(mountPoint, fileFooter) {
            FileStream = fileStream;
        }

        public FileFooter FileFooter { get; protected set; }
        public Stream? FileStream { get; set; }

        public string MountPoint { get; protected set; }
        public List<Record> Records { get; } = new List<Record>();
        public IEnumerator<Record> GetEnumerator() {
            return Records.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return ((IEnumerable) Records).GetEnumerator();
        }

        public IEnumerable<FileInfo> UnpackAll(DirectoryInfo unpackRoot) {
            return UnpackAll(FileStream, unpackRoot);
        }

        public IEnumerable<FileInfo> UnpackAll(Stream pakFile, DirectoryInfo unpackRoot) {
            return Records.Select(fileRecord => fileRecord.Unpack(pakFile, unpackRoot));
        }

        private FileInfo? Unpack(string filePath, DirectoryInfo unpackRoot) {
            return Unpack(filePath, FileStream, unpackRoot);
        }

        private FileInfo? Unpack(string filePath, Stream pakFile, DirectoryInfo unpackRoot) {
            var matchedFile = Records.FirstOrDefault(r => r.FileName.ToLower().TrimStart('/') == filePath);
            return matchedFile?.Unpack(pakFile, unpackRoot);
        }
    }
}
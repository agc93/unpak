using System;
using System.IO;
using System.Linq;
using UnPak.Core;
using static System.Console;

namespace UnPak.Console
{
    class Program
    {
        static void Main(string[] args) {
            var fi = new FileInfo(@"X:/Staging/Screenshot Mode_P.pak");
            using var fs = fi.OpenRead();
            var readerProvider = new PakFileProvider(new[] {new PakVersion3Format()}, null);
            var reader = readerProvider.GetReader(fs);
            var file = reader.ReadFile();
            WriteLine($"Records: {file.Records.Count}");
            WriteLine($"Mount Point: {file.MountPoint}");
            foreach (var fileRecord in file.Records) {
                WriteLine($"{fileRecord.FileName} ({fileRecord.RawSize} bytes at 0x{fileRecord.RecordOffset:x8})");
            }

            var di = new DirectoryInfo(@"X:\Tools\u4pak-test\unpack");
            var files = file.UnpackAll(fs, di).ToList();

            
            var upFile = new FileInfo(@"X:\Tools\u4pak-test\upk_P.pak");
            var writer = readerProvider.GetWriter();
            
            writer.BuildFromDirectory(di.GetDirectories().First(), upFile);

            using (var reReader = readerProvider.GetReader(upFile)) {
                var diu = new DirectoryInfo(@"X:\Tools\u4pak-test\upk");
                var reFile = reReader.ReadFile();
                var repacks = reFile.UnpackAll(upFile.OpenRead(), diu).ToList();
            }
        }
    }
}

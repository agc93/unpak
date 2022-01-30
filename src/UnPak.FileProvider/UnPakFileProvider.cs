using System.IO;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using UnPak.Core;

namespace UnPak.FileProvider
{
    public class UnPakFileProvider : IFileProvider
    {
        private readonly PakFile _pakFile;
        private readonly string _contentRoot;
        private readonly string _virtualPrefix;
        private readonly FileInfo _sourceFile;

        public UnPakFileProvider(FileInfo sourceFile, UnPakFileProviderConfiguration? config = null) {
            _sourceFile = sourceFile;
            var crypto = new Core.Crypto.ManagedHashProvider();
            var provider =
                new PakFileProvider(
                    new IPakFormat[]
                        { new PakVersion3Format(crypto), new PakVersion4Format(), new PakVersion8Format(crypto) },
                    new IFooterLayout[] { new DefaultFooterLayout(), new PaddedFooterLayout() }, crypto);
            var reader = provider.GetReader(_sourceFile);
            _pakFile = reader.ReadFile();
            _contentRoot = config?.ContentRoot ?? string.Empty;
            _virtualPrefix = config?.VirtualPrefix ?? string.Empty;
        }
        
        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            return new PakDirectoryContents(_pakFile, Path.Combine(_contentRoot, subpath.StripPrefix(_virtualPrefix)));
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            return new PakFileInfo(_pakFile, Path.Combine(_contentRoot, subpath.StripPrefix(_virtualPrefix))) {
                LastModified = _sourceFile.LastWriteTime
            };
        }

        public IChangeToken? Watch(string filter) => null;
    }
}
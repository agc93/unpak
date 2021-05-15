using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using UnPak.Core;

namespace UnPak.Console
{
    public class UnpackCommand : AsyncCommand<UnpackCommand.Settings>
    {
        private readonly ILogger<UnpackCommand> _logger;
        private readonly PakFileProvider _provider;

        public class Settings : CommandSettings {
            [CommandArgument(0, "[fileName]")]
            [Description("Path to the target PAK file to unpack.")]
            public string TargetFilePath {get;set;}

            [CommandOption("--output [outputPath]")]
            [Description("Target path to unpack the files to.")]
            public FlagValue<string> UnpackRootPath {get;set;}
            
            [CommandOption("--version [archiveVersion]")]
            [Description("Archive version of the target file. Defaults to '3'.")]
            public FlagValue<int?> ArchiveVersion {get;set;}
            
            [CommandOption("--magic [fileMagic]")]
            [Description("Overrides the default file magic.")]
            public FlagValue<int?> FileMagic {get;set;}
        }

        public UnpackCommand(ILogger<UnpackCommand> logger, PakFileProvider provider) {
            _logger = logger;
            _provider = provider;
        }

        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings) {
            var unpackTarget = settings.UnpackRootPath.OrDefault(Path.Combine(System.Environment.CurrentDirectory,
                Path.GetFileNameWithoutExtension(settings.TargetFilePath)));
            _logger.LogInformation($"Unpacking PAK file at '{settings.TargetFilePath}' to '{unpackTarget}'");
            var opts = new PakLayoutOptions();
            if (settings.FileMagic.IsSet && uint.TryParse(settings.FileMagic.Value.ToString(), out var fileMagic)) {
                opts = new PakLayoutOptions {Magic = fileMagic};
            }
            using var reader = _provider.GetReader(new FileInfo(settings.TargetFilePath), opts);
            var pakFile = reader.ReadFile();
            _logger.LogInformation($"Read v{pakFile.FileFooter.Version} PAK file index: {pakFile.Records.Count} records mounted at '{pakFile.MountPoint}'.");
            var files = reader.UnpackTo(new DirectoryInfo(unpackTarget), pakFile);
            _logger.LogInformation($"[bold green]Completed![/] Successfully unpacked {files.Count} files to '{Path.GetDirectoryName(unpackTarget)}'");
            return files.Any() ? 0 : 500;
        }
    }
}
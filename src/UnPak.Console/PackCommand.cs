using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using UnPak.Core;

namespace UnPak.Console
{
    public class PackCommand : AsyncCommand<PackCommand.Settings>
    {
        private readonly ILogger<PackCommand> _logger;
        private readonly PakFileProvider _fileProvider;

        public class Settings : CommandSettings {
            [CommandArgument(0, "[fileRoot]")]
            [Description("Directory containing your mod files (aka Nimbus folder)")]
            public string FileRootPath {get;set;} = System.Environment.CurrentDirectory;

            [CommandArgument(1, "[outputFileName]")]
            [Description("The target pak file to generate.")]
            public string TargetFilePath {get;set;}

            [CommandOption("--version <archiveVersion>")]
            public FlagValue<int?> ArchiveVersion { get; set; }
            
            [CommandOption("--mount-point <mount-point>")]
            public FlagValue<string> MountPoint { get; set; }
        }

        public PackCommand(ILogger<PackCommand> logger, PakFileProvider fileProvider) {
            _logger = logger;
            _fileProvider = fileProvider;
        }

        public override Task<int> ExecuteAsync(CommandContext context, Settings settings) {
            var writer = _fileProvider.GetWriter();
            var opts = new PakFileCreationOptions(settings.ArchiveVersion.OrDefault(null), settings.MountPoint.OrDefault(), null);
            _logger.LogInformation($"Creating new PAK archive with version '{opts.ArchiveVersion} at '{opts.MountPoint}' (magic: '{opts.Magic:x8}')");
            var result = writer.BuildFromDirectory(new DirectoryInfo(settings.FileRootPath), new FileInfo(settings.TargetFilePath), opts);
            _logger.LogInformation($"[bold green]SUCCESS![/] Files successfully packed to '{Path.GetFileName(settings.TargetFilePath)}'.");
            return Task.FromResult(result.Exists && result.Length > 0 ? 0 : 500);
        }
    }
}
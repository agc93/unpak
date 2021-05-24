using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;
using UnPak.Core;

namespace UnPak.Console
{
    public class PackCommand : AsyncCommand<PackCommand.Settings>
    {
        private readonly ILogger<PackCommand> _logger;
        private readonly PakFileProvider _fileProvider;

        public class Settings : CommandSettings {
            
            public override ValidationResult Validate() {
                FileRootPath = FileRootPath.TrimEnd('\\', '"');
                /*if (!Debugger.IsAttached) {
                    Debugger.Launch();
                }*/
                var dirSrv = new DirectoryService();
                try {
                    if (!DisableAutoRoot.IsSet || !DisableAutoRoot.Value) {
                        var packRoot = dirSrv.GetPackRoot(new DirectoryInfo(FileRootPath));
                        //FileRootPath = packRoot.FullName;
                        if (!TargetFilePath.IsSet || string.IsNullOrWhiteSpace(TargetFilePath.Value)) {
                            TargetFilePath.Value = dirSrv.GetTargetName(packRoot);
                        }
                    }
                    return ValidationResult.Success();
                }
                catch {
                    return ValidationResult.Error();
                }

                
            }

            [CommandArgument(0, "<fileRoot>")]
            [Description("Directory containing your mod files (aka Nimbus folder)")]
            public string FileRootPath {get;set;} = System.Environment.CurrentDirectory;

            [CommandOption("--output-name <output-name>")]
            [Description("The target pak file to generate.")]
            public FlagValue<string> TargetFilePath {get;set;}

            [CommandOption("--version <archiveVersion>")]
            public FlagValue<int?> ArchiveVersion { get; init; }
            
            [CommandOption("--mount-point <mount-point>")]
            public FlagValue<string> MountPoint { get; init; }
            
            [CommandOption("--no-auto")]
            public FlagValue<bool> DisableAutoRoot { get; init; }
        }

        public PackCommand(ILogger<PackCommand> logger, PakFileProvider fileProvider) {
            _logger = logger;
            _fileProvider = fileProvider;
        }

        public override Task<int> ExecuteAsync(CommandContext context, Settings settings) {
            var writer = _fileProvider.GetWriter();
            var opts = new PakFileCreationOptions(settings.ArchiveVersion.OrDefault(null), settings.MountPoint.OrDefault(), null);
            _logger.LogInformation($"Creating new PAK archive with version '{opts.ArchiveVersion}' at '{opts.MountPoint}' (magic: '{opts.Magic:x8}')");
            var result = writer.BuildFromDirectory(new DirectoryInfo(settings.FileRootPath), new FileInfo(settings.TargetFilePath.Value), opts);
            _logger.LogInformation($"[bold green]SUCCESS![/] Files successfully packed to '{Path.GetFileName(settings.TargetFilePath.Value)}'.");
            return Task.FromResult(result.Exists && result.Length > 0 ? 0 : 500);
        }
    }
}
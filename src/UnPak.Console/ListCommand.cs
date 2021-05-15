using System.ComponentModel;
using System.IO;
using Humanizer;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;
using UnPak.Core;

namespace UnPak.Console
{
    public class ListCommand : Command<ListCommand.Settings>
    {
        private readonly ILogger<ListCommand> _logger;
        private readonly PakFileProvider _provider;
        private readonly IAnsiConsole _console;

        public class Settings : ConsoleSettings {
            [CommandArgument(0, "[fileName]")]
            [Description("Path to the target PAK file to unpack.")]
            public string TargetFilePath {get;set;}
            
            [CommandOption("--version [archiveVersion]")]
            [Description("Archive version of the target file. Defaults to '3'.")]
            public FlagValue<int?> ArchiveVersion {get;set;}
            
            [CommandOption("--magic [fileMagic]")]
            [Description("Overrides the default file magic.")]
            public FlagValue<int?> FileMagic {get;set;}
        }

        public ListCommand(ILogger<ListCommand> logger, PakFileProvider provider, IAnsiConsole console) {
            _logger = logger;
            _provider = provider;
            _console = console;
        }

        public override int Execute(CommandContext context, Settings settings) {
            using var reader = _provider.GetReader(new FileInfo(settings.TargetFilePath));
            var file = reader.ReadFile();
            _logger.LogInformation($"Read valid v{file.FileFooter.Version} PAK file at '{Path.GetFileName(settings.TargetFilePath)}");
            _logger.LogInformation($"Mount point: {file.MountPoint} | Magic: {file.FileFooter.Magic:x8}");
            _logger.LogInformation($"File contains {file.Records.Count} records.");
            if (settings.OutputMode == OutputMode.None) {
                var table = new Table().AddColumn("File").AddColumn("Offset").AddColumn("Size");
                foreach (var fileRecord in file.Records) {
                    table.AddRow(fileRecord.FileName, fileRecord.DataOffset.ToString(),
                        ((long) fileRecord.CompressedSize).Bytes().ToString());
                }
                _console.Render(table);
            }
            return 0;
        }
    }
}
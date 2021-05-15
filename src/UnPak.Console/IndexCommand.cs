using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Spectre.Cli.AppInfo;
using Spectre.Console.Cli;
using static System.Console;
using static UnPak.Console.Startup;

namespace UnPak.Console
{
    public class IndexCommand : AsyncCommand<IndexCommand.Settings>
    {
        private readonly ILogger<IndexCommand> _logger;

        public IndexCommand(ILogger<IndexCommand> logger) {
            _logger = logger;
        }
        public class Settings : CommandSettings {
            [CommandArgument(0, "<folder-path>")]
            public string FolderPath {get;set;}
        }

        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings) {
            var result = 0;
            if (settings.FolderPath == null) {
                WriteLine("No path provided!");
                throw new ArgumentNullException(nameof(settings.FolderPath));
            }
            else if (File.Exists(settings.FolderPath) && Path.GetExtension(settings.FolderPath) is var fileExt && fileExt == ".pak") {
                //time to unpack boys
                var app = GetApp();
                var args = new[] { "unpack", settings.FolderPath}.Concat(context.Remaining.Raw);
                result = await app.RunAsync(args);
            } else if (Directory.Exists(settings.FolderPath) && Directory.EnumerateFileSystemEntries(settings.FolderPath).Any()) {
                var di = new DirectoryInfo(settings.FolderPath);
                //time to pack boys
                var app = GetApp();
                var info = new AppInfoService();
                var args = new[] { "pack", settings.FolderPath}.Concat(context.Remaining.Raw);
                result = await app.RunAsync(args);
                // }
                // return await app.RunAsync(args);
            }
            if (result != 0) {
                WriteLine("It looks like there might have been an error running the pack command!");
                WriteLine("You can press <ENTER> to close, or copy/screenshot any errors you find above to help isolating any bugs.");
                WriteLine(string.Empty.PadLeft(9) + "Press <ENTER> to continue...");
                ReadLine();
            }
            return result;
        }
    }
}
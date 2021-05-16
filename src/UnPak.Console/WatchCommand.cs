using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Spectre.Console.Cli;

namespace UnPak.Console
{
    [Description("Watches a folder and automatically packs it when changes are detected")]
    public class WatchCommand : AsyncCommand<WatchCommand.Settings>
    {
        public class Settings : ConsoleSettings
        {
            [CommandArgument(0, "[fileRoot]")]
            [Description("Directory containing your mod files (aka Nimbus folder)")]
            public string FileRootPath {get;set;} = System.Environment.CurrentDirectory;

            [CommandArgument(1, "[outputFileName]")]
            [Description("The target pak file to generate.")]
            public string TargetFilePath {get;set;}
        }

        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings) {
            var cts = new CancellationTokenSource();
            System.Console.CancelKeyPress += (_, e) => cts.Cancel();
            var watcher = new WatcherService(settings.FileRootPath, settings.TargetFilePath);
            await watcher.Start(cts.Token);
            return 0;
        }
    }
}
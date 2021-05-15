using System.ComponentModel;
using Spectre.Console.Cli;

namespace UnPak.Console
{
    public enum OutputMode {
        None,
        Silent,
        Text,
        Json
    }
    public class ConsoleSettings : CommandSettings
    {
        [CommandOption("-o|--output <mode>")]
        [Description("Specify the output format for parsed data.")]
        public OutputMode OutputMode {get;set;} = OutputMode.None;
    }
}
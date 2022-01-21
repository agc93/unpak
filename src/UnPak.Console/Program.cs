using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Spectre.Console.Cli;
using UnPak.Core;
using static System.Console;

namespace UnPak.Console
{
    class Program
    {
        static async Task<int> Main(string[] args) {
            #if DEBUG
            JetBrains.Profiler.Api.MeasureProfiler.StartCollectingData();
            #endif
            var app = Startup.GetApp();
            app.SetDefaultCommand<IndexCommand>();
            var ret = await app.RunAsync(args);
            #if DEBUG
            JetBrains.Profiler.Api.MeasureProfiler.SaveData();
            #endif
            return ret;
        }
    }
}

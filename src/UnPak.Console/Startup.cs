using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Cli.AppInfo;
using Spectre.Cli.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;
using UnPak.Core;

// ReSharper disable StringLiteralTypo

namespace UnPak.Console
{
    public static class Startup
    {
        internal static CommandApp GetApp() {
            var app = new CommandApp(new DependencyInjectionRegistrar(GetServices()));
            // app.SetDefaultCommand<PackCommand>();
            app.Configure(c => {
                c.PropagateExceptions();
                c.SetApplicationName("upk");
                c.AddCommand<PackCommand>("pack");
                c.AddCommand<UnpackCommand>("unpack");
                c.AddCommand<ListCommand>("list");
                c.AddExample(new[] { "pack", "./ProjectWingman" });
                c.AddExample(new[] { "pack", "C:\\Mods\\GoldNozzlesForLife\\Nimbus" });
                c.AddExample(new[] { "unpack", "./ClearlyInferiorMobiusSkins.pak" });
                c.AddExample(new []{ "unpack", "C:\\SuperSecretFiles\\Cursed\\FD14_Tacmot.pak"});
            });
            return app;
        }
        
        internal static IServiceCollection GetServices() {
            var services = new ServiceCollection();
            services.AddSingleton<AppInfoService>();
            services.AddRuntimeServices();
            if (OperatingSystem.IsWindows()) {
                services.AddSingleton<UnPak.Core.Crypto.IHashProvider, UnPak.Core.Crypto.NativeHashProvider>();    
            }
            else {
                services.AddSingleton<UnPak.Core.Crypto.IHashProvider, UnPak.Core.Crypto.ManagedHashProvider>();
            }
            
            
            services.AddSingleton<IAnsiConsole>(p => {
                return AnsiConsole.Create(
                    new AnsiConsoleSettings {
                        Ansi = Spectre.Console.AnsiSupport.Detect,
                        ColorSystem = ColorSystemSupport.Detect
                    });
            });
            services.AddLogging(logging => {
                var level = GetLogLevel();
                logging.SetMinimumLevel(LogLevel.Trace);
                logging.AddInlineSpectreConsole(c => {
                    c.LogLevel = level;
                });
                AddFileLogging(logging, level);
            });
            /*services.Scan(scan =>
                scan.FromAssemblyOf<Identifier>()
                    .AddClasses(classes => classes.AssignableTo(typeof(AceCore.Parsers.IIdentifierParser))).AsImplementedInterfaces().WithSingletonLifetime()
            );*/
            return services;
        }

        internal static IServiceCollection AddRuntimeServices(this IServiceCollection services) {
            services.AddSingleton<PakFileProvider>();
            services.Scan(scan =>
            {
                scan.FromAssemblyOf<PakFileProvider>()
                    .AddClasses(classes => classes.AssignableTo<IPakFormat>()).AsImplementedInterfaces()
                    .AddClasses(classes => classes.AssignableTo<IFooterLayout>()).AsImplementedInterfaces()
                    .WithSingletonLifetime();
            });
            return services;
        }
        
        internal static ILoggingBuilder AddFileLogging(ILoggingBuilder logging, LogLevel level) {
            var options = new NReco.Logging.File.FileLoggerOptions {
                Append = true,
                FileSizeLimitBytes = 4096,
                MaxRollingFiles = 5
            };
            if (level < LogLevel.Information) {
                logging.Services.Add(ServiceDescriptor.Singleton<ILoggerProvider, NReco.Logging.File.FileLoggerProvider>(
                    (srvPrv) => {
                        return new NReco.Logging.File.FileLoggerProvider("unpak.log", options) { MinLevel = level };
                    }
                ));
            }
            return logging;
        }

        internal static LogLevel GetLogLevel() {
            var envVar = System.Environment.GetEnvironmentVariable("UNPAK_DEBUG");
            if (System.IO.File.Exists(System.IO.Path.Combine(Environment.CurrentDirectory, "unpak-debug.txt"))) envVar = "trace";
            if (System.IO.File.Exists(System.IO.Path.Combine(new System.IO.FileInfo(CurrentPath()).Directory.FullName, "unpak-debug.txt"))) envVar = "trace";
            return string.IsNullOrWhiteSpace(envVar)
                ? LogLevel.Information
                :  envVar.ToLower() == "trace"
                    ? LogLevel.Trace
                    : LogLevel.Debug;
        }

        private static string CurrentPath() {
            return System.AppContext.BaseDirectory;
            //System.Reflection.Assembly.GetEntryAssembly().Location
        }
    }
}
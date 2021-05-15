using System;
using Spectre.Console.Cli;

namespace UnPak.Console
{
    public static class ConsoleExtensions
    {
        internal static T OrDefault<T>(this FlagValue<T> option) {
            return OrDefault<T>(option, default(T));
        }

        internal static T OrDefault<T>(this FlagValue<T> option, T defaultValue) {
            return option.IsSet ? option.Value : defaultValue;
        }
    }
}
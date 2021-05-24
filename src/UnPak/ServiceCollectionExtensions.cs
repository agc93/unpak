using System;
using Microsoft.Extensions.DependencyInjection;
using UnPak.Core;

namespace UnPak
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddUnPakServices(this IServiceCollection services) {
            services.AddSingleton<PakFileProvider>();
            services.AddSingleton<IPakFormat, PakVersion3Format>();
            services.AddSingleton<IPakFormat, PakVersion4Format>();
            if (OperatingSystem.IsWindows()) {
                services.AddSingleton<Core.Crypto.IHashProvider, Core.Crypto.NativeHashProvider>();    
            }
            else {
                services.AddSingleton<Core.Crypto.IHashProvider, Core.Crypto.ManagedHashProvider>();
            }
            return services;
        }
    }
}
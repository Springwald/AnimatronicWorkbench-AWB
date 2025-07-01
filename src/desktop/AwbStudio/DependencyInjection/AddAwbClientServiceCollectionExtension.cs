// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Threading.Tasks;

namespace AwbStudio.DependencyInjection
{
    internal static class AddAwbClientServiceCollectionExtension
    {
        public static void AddAwbClientService(this IServiceCollection services)
        {
            // add the AwbClientsService as a singleton of IAwbClientsService and call the InitAsync method when initializing the service
            services.TryAddSingleton<IAwbClientsService>(sp =>
            {
                var service = new AwbClientsService(sp.GetService<IAwbLogger>());
                Task.Run(() => service.InitAsync());
                return service;
            });

        }
    }
}

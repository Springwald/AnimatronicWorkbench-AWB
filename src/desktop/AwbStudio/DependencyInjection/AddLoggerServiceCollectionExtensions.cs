// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace AwbStudio.DependencyInjection
{
    internal static class AddLoggerServiceCollectionExtensions
    {
        public static void AddILoggerServices(this IServiceCollection services)
        {
            services.TryAddSingleton<IAwbLogger>(sp =>
            {
                var debugWindow = sp.GetService<DebugWindow>();
                if (debugWindow != null)
                {
                    debugWindow.Show();
                    return new AwbDebugWindowLogger(debugWindow);
                } else
                {
                    return new AwbLoggerConsole(throwWhenInDebugMode: false);
                }
                
            });
        }
    }
}

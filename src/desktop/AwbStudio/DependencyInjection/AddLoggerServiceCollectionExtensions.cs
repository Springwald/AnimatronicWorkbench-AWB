// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Services;
using AwbStudio.UserControls;
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
                }
                else
                {
                    return new AwbLoggerConsole(throwWhenInDebugMode: false);
                }

            });
        }
    }
}

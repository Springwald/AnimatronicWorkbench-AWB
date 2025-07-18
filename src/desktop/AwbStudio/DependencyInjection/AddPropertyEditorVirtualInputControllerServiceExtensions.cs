﻿// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Services;
using AwbStudio.TimelineControls.PropertyControls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace AwbStudio.DependencyInjection
{
    internal static class AddPropertyEditorVirtualInputControllerServiceExtensions
    {
        public static void AddPropertyEditorVirtualInputControllerService(this IServiceCollection services)
        {
            services.TryAddSingleton<IPropertyEditorVirtualInputController>(sp =>
            {
                var logger = sp.GetRequiredService<IAwbLogger>();
                return new PropertyEditorVirtualInputController(logger: logger);
            });
        }
    }
}

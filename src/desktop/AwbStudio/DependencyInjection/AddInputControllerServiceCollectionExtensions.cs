// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Services;
using Awb.Core.Tools;
using AwbStudio.TimelineControls.PropertyControls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AwbStudio.DependencyInjection
{
    internal static class AddInputControllerServiceCollectionExtensions
    {
        public static void AddInputControllerServices(this IServiceCollection services)
        {
            services.TryAddSingleton<IInputControllerService>(sp =>
            {
                var logger = sp.GetRequiredService<IAwbLogger>();
                var propertyEditorVirtualInputController = sp.GetRequiredService<IPropertyEditorVirtualInputController>();
                var invokerService = sp.GetRequiredService<IInvokerService>();

                return new InputControllerService(
                    logger: logger,
                    invokerService: invokerService,
                    additionalTimelineControllers: [propertyEditorVirtualInputController]
                    );
            });
        }


    }
}

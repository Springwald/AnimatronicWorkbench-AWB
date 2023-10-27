// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.InputControllers.TimelineInputControllers;
using Awb.Core.Services;
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

                var valueTuningWindow = new ValueTuningWindow();
                valueTuningWindow.Show();

                return new InputControllerService(
                    logger: logger, 
                    additionalTimelineControllers: new ITimelineController[] { valueTuningWindow });
            });
        }


    }
}

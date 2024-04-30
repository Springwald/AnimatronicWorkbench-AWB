// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.InputControllers.TimelineInputControllers;
using Awb.Core.Services;
using System;
using System.Threading.Tasks;

namespace AwbStudio.TimelineControls.PropertyControls
{

    public interface IPropertyEditorVirtualInputController : ITimelineController
    {
    }

    public class PropertyEditorVirtualInputController : IPropertyEditorVirtualInputController
    {
        private IAwbLogger _logger;

        public PropertyEditorVirtualInputController(IAwbLogger logger)
        {
            this._logger = logger;
        }

        public event EventHandler<TimelineControllerEventArgs>? OnTimelineEvent;

        public async Task SetActuatorValueAsync(int index, double valueInPercent)
        {
            await Task.CompletedTask;
        }

        public async Task SetPlayStateAsync(ITimelineController.PlayStates playStates)
        {
            await Task.CompletedTask;
        }

        public async Task ShowPointButtonStateAsync(int index, bool pointExists)
        {
            await Task.CompletedTask;
        }

        public void Dispose()
        {
        }
    }
}

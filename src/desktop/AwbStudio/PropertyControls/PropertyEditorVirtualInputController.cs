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

        public async Task SetActuatorValue(int index, double valueInPercent)
        {

        }

        public async Task SetPlayState(ITimelineController.PlayStates playStates)
        {
        }

        public async Task ShowPointButtonState(int index, bool pointExists)
        {
        }

        public void Dispose()
        {
        }
    }
}

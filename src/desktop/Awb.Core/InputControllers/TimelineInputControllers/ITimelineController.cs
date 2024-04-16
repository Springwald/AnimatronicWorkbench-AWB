// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using System;

namespace Awb.Core.InputControllers.TimelineInputControllers
{
    public interface ITimelineController : IDisposable
    {
        public enum PlayStates
        {
            Editor,
            Playing
        }

        event EventHandler<TimelineControllerEventArgs> OnTimelineEvent;

        Task SetPlayState(PlayStates playStates);

        Task SetActuatorValue(int index, double valueInPercent);

        Task ShowPointButtonState(int index, bool pointExists);
    }
}

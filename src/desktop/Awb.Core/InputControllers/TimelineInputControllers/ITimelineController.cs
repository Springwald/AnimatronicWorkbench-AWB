// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

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

        Task SetPlayStateAsync(PlayStates playStates);

        Task SetActuatorValueAsync(int index, double valueInPercent);

        Task ShowPointButtonStateAsync(int index, bool pointExists);
    }
}

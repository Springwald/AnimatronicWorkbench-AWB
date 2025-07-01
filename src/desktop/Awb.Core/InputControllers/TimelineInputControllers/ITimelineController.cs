// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

namespace Awb.Core.InputControllers.TimelineInputControllers
{
    public interface ITimelineController : IDisposable
    {
        public enum PlayStates
        {
            Editor,
            Playing
        }

        event EventHandler<TimelineControllerEventArgs>? OnTimelineEvent;

        Task SetPlayStateAsync(PlayStates playStates);

        Task SetActuatorValueAsync(int index, double valueInPercent);

        Task ShowPointButtonStateAsync(int index, bool pointExists);
    }
}

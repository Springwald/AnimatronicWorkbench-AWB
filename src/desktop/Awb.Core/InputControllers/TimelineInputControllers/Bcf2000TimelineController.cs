// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.InputControllers.BCF2000;

namespace Awb.Core.InputControllers.TimelineInputControllers
{
    internal class Bcf2000TimelineController : ITimelineController, IDisposable
    {
        private ITimelineController.PlayStates _playState = ITimelineController.PlayStates.Editor;
        private readonly Bcf2000Controller _bcf2000Controller;

        public string?[] ActualActuatorNames { set { } }

        public Bcf2000TimelineController(Bcf2000Controller bcf2000Controller)
        {
            _bcf2000Controller = bcf2000Controller;
            if (bcf2000Controller != null)
            {
                bcf2000Controller.ActionReceived += Bcf2000Controller_ActionReceived;
            }
        }

        public event EventHandler<TimelineControllerEventArgs>? OnTimelineEvent;

        public void Dispose()
        {
            if (_bcf2000Controller != null)
            {
                _bcf2000Controller.ActionReceived -= Bcf2000Controller_ActionReceived;
            }
        }

        public async Task SetPlayStateAsync(ITimelineController.PlayStates playState)
        {
            _playState = playState;
            await Task.CompletedTask;
        }

        public async Task SetActuatorValueAsync(int index, double valueInPercent)
        {
            await _bcf2000Controller.SetFaderPositionAsync((byte)(index + 1), (byte)(Math.Max(0, Math.Min(127, valueInPercent * 127 / 100.0))));
        }

        public async Task ShowPointButtonStateAsync(int index, bool pointExists)
        {
            //_bcf2000Controller.SetButtonLedState(topLine: true, (byte)(index + 1), pointExists ? LedState.On : LedState.Off);
            await Task.CompletedTask;
        }

        private async void Bcf2000Controller_ActionReceived(object? sender, Bcf2000EventArgs e)
        {
            if (OnTimelineEvent == null) return;

            switch (e.InputType)
            {
                case Bcf2000EventArgs.InputTypes.Unknown:
                case Bcf2000EventArgs.InputTypes.KnobRotation:
                case Bcf2000EventArgs.InputTypes.KnobPress:
                    break;

                case Bcf2000EventArgs.InputTypes.Fader:
                    OnTimelineEvent.Invoke(this, new TimelineControllerEventArgs(
                        TimelineControllerEventArgs.EventTypes.ActuatorValueChanged,
                        actuatorIndex: e.InputIndex - 1,
                        valueInPercent: e.Value / 127.0 * 100));
                    break;

                case Bcf2000EventArgs.InputTypes.ButtonTopLine:
                    if (e.Value == 127)
                        OnTimelineEvent.Invoke(this, new TimelineControllerEventArgs(TimelineControllerEventArgs.EventTypes.ActuatorTogglePoint, e.InputIndex - 1, e.Value));
                    break;

                case Bcf2000EventArgs.InputTypes.ButtonBottomLine:
                    if (e.Value == 127)
                        switch (e.InputIndex)
                        {
                            case 3: // << button
                                switch (_playState)
                                {
                                    case ITimelineController.PlayStates.Editor:
                                        OnTimelineEvent.Invoke(this, new TimelineControllerEventArgs(TimelineControllerEventArgs.EventTypes.PreviousPage));
                                        break;
                                    case ITimelineController.PlayStates.Playing:
                                        OnTimelineEvent.Invoke(this, new TimelineControllerEventArgs(TimelineControllerEventArgs.EventTypes.Backwards));
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException($"{nameof(_playState)}:{_playState}");
                                }
                                break;
                            case 4: // >> button
                                switch (_playState)
                                {
                                    case ITimelineController.PlayStates.Editor:
                                        OnTimelineEvent.Invoke(this, new TimelineControllerEventArgs(TimelineControllerEventArgs.EventTypes.NextPage));
                                        break;
                                    case ITimelineController.PlayStates.Playing:
                                        OnTimelineEvent.Invoke(this, new TimelineControllerEventArgs(TimelineControllerEventArgs.EventTypes.Forward));
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException($"{nameof(_playState)}:{_playState}");
                                }

                                break;
                            case 6: // stop button
                                OnTimelineEvent.Invoke(this, new TimelineControllerEventArgs(TimelineControllerEventArgs.EventTypes.Stop));
                                break;
                            case 7: // play button
                                OnTimelineEvent.Invoke(this, new TimelineControllerEventArgs(TimelineControllerEventArgs.EventTypes.Play));
                                break;
                        }
                    break;


                default:
                    throw new ArgumentOutOfRangeException($"{nameof(e.InputType)}:{e.InputType}");

            }
            await Task.CompletedTask;
        }
    }
}

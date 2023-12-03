// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.InputControllers.XTouchMini;

namespace Awb.Core.InputControllers.TimelineInputControllers
{
    internal class XTouchMiniTimelineController : ITimelineController, IDisposable
    {
        private readonly XTouchMiniController _xTouchMiniController;
        private ITimelineController.PlayStates _playState = ITimelineController.PlayStates.Editor;

        public string?[] ActualActuatorNames { set { } }

        public XTouchMiniTimelineController(XTouchMiniController xTouchMiniController)
        {
            _xTouchMiniController = xTouchMiniController;
            if (_xTouchMiniController != null)
            {
                _xTouchMiniController.ActionReceived += XTouchMiniController_ActionReceived;
            }
        }

        public event EventHandler<TimelineControllerEventArgs>? OnTimelineEvent;

        public void Dispose()
        {
            if (_xTouchMiniController != null)
            {
                _xTouchMiniController.ActionReceived -= XTouchMiniController_ActionReceived;
            }
        }

        public async Task SetPlayState(ITimelineController.PlayStates playState)
        {
            _playState = playState;
            switch (_playState)
            {
                case ITimelineController.PlayStates.Editor:
                    _xTouchMiniController.SetButtonLedState(topLine: false, button: 7, ledState: LedState.Off);
                    break;
                case ITimelineController.PlayStates.Playing:
                    _xTouchMiniController.SetButtonLedState(topLine: false, button: 7, ledState: LedState.Blinking);
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"{nameof(_playState)}:{_playState}");
            }
            await Task.CompletedTask;
        }

        public async Task SetActuatorValue(int index, double valueInPercent)
        {
            var ok = _xTouchMiniController.SetKnobPosition((byte)(index + 1), (byte)(Math.Max(0, Math.Min(127, valueInPercent * 127 / 100.0))));
            await Task.CompletedTask;
        }



        public async Task ShowPointButtonState(int index, bool pointExists)
        {
            var ok = _xTouchMiniController.SetButtonLedState(topLine: true, (byte)(index + 1), pointExists ? LedState.On : LedState.Off);
            await Task.CompletedTask;
        }

        private async void XTouchMiniController_ActionReceived(object? sender, XTouchMiniEventArgs e)
        {
            if (OnTimelineEvent == null) return;

            switch (e.InputType)
            {
                case XTouchMiniEventArgs.InputTypes.Unknown:
                    break;

                case XTouchMiniEventArgs.InputTypes.KnobRotation:
                    OnTimelineEvent.Invoke(this, new TimelineControllerEventArgs(
                        TimelineControllerEventArgs.EventTypes.ActuatorValueChanged,
                        actuatorIndex: e.InputIndex - 1,
                        valueInPercent: e.Value / 127.0 * 100));
                    break;

                case XTouchMiniEventArgs.InputTypes.KnobPress:
                    break;

                case XTouchMiniEventArgs.InputTypes.ButtonTopLine:
                    if (e.Value == 127)
                        OnTimelineEvent.Invoke(this, new TimelineControllerEventArgs(TimelineControllerEventArgs.EventTypes.ActuatorTogglePoint, e.InputIndex - 1, e.Value));
                    break;

                case XTouchMiniEventArgs.InputTypes.ButtonBottomLine:

                    switch (e.InputIndex)
                    {
                        case 1: // Scroll / paging button 
                            switch (_playState)
                            {
                                case ITimelineController.PlayStates.Editor:
                                    OnTimelineEvent.Invoke(this, new TimelineControllerEventArgs(TimelineControllerEventArgs.EventTypes.PreviousPage));
                                    break;
                                case ITimelineController.PlayStates.Playing:
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException($"{nameof(_playState)}:{_playState}");
                            }
                            break;

                        case 2:// bank switch button
                            if (e.Value == 127)// 127=pressed, 0=released
                                switch (_playState)
                                {
                                    case ITimelineController.PlayStates.Editor:
                                    case ITimelineController.PlayStates.Playing:
                                        OnTimelineEvent.Invoke(this, new TimelineControllerEventArgs(TimelineControllerEventArgs.EventTypes.NextBank));
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException($"{nameof(_playState)}:{_playState}");
                                }
                            break;

                        case 3: // << button
                            if (e.Value == 127)// 127=pressed, 0=released
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
                            if (e.Value == 127)// 127=pressed, 0=released
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
                        case 7: // play / stop button
                            if (e.Value == 0) // 127=pressed, 0=released
                                switch (_playState)
                                {
                                    case ITimelineController.PlayStates.Editor:
                                        OnTimelineEvent.Invoke(this, new TimelineControllerEventArgs(TimelineControllerEventArgs.EventTypes.Play));
                                        break;
                                    case ITimelineController.PlayStates.Playing:
                                        OnTimelineEvent.Invoke(this, new TimelineControllerEventArgs(TimelineControllerEventArgs.EventTypes.Stop));
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException($"{nameof(_playState)}:{_playState}");
                                }
                            break;
                        case 8: // save button
                            if (e.Value == 127) // 127=pressed, 0=released
                                switch (_playState)
                                {
                                    case ITimelineController.PlayStates.Editor:
                                        OnTimelineEvent.Invoke(this, new TimelineControllerEventArgs(TimelineControllerEventArgs.EventTypes.Save));
                                        break;
                                    case ITimelineController.PlayStates.Playing:
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException($"{nameof(_playState)}:{_playState}");
                                }
                            break;
                    }
                    break;

                case XTouchMiniEventArgs.InputTypes.MainFader:
                    OnTimelineEvent.Invoke(this, new TimelineControllerEventArgs(
                          TimelineControllerEventArgs.EventTypes.PlayPosAbsoluteChanged,
                          actuatorIndex: -1,
                          valueInPercent: e.Value / 127.0 * 100));
                    break;

                default:
                    throw new ArgumentOutOfRangeException($"{nameof(e.InputType)}:{e.InputType}");

            }
            await Task.CompletedTask;
        }
    }
}

// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Actuators;
using Awb.Core.InputControllers.TimelineInputControllers;
using Awb.Core.Player;
using Awb.Core.Services;
using Awb.Core.Timelines;
using System;
using System.Linq;

namespace AwbStudio.TimelineEditing
{
    internal class TimelineEventHandling : IDisposable
    {
        private readonly TimelineData _timelineData;
        private readonly TimelinePlayer _timelinePlayer;
        private readonly IActuatorsService _actuatorsService;
        private readonly PlayPosSynchronizer _playPosSynchronizer;
        private readonly ITimelineController[] _timelineControllers;
        private readonly TimelineControllerPlayViewPos _timelineControllerPlayViewPos;
        private readonly TimelineViewContext _viewContext;
        private readonly TimelineEditingManipulation _timelineEditingManipulation;
        private readonly IActuator[] _allActuators;
        private readonly IActuator[] _controllerTuneableActuators;
        private volatile bool _manualUpdatingValuesViaController;
        private int _lastActuatorChanged = 1; // prevent double actuator change events to the midi controller

        public TimelineEventHandling(TimelineData timelineData, TimelineControllerPlayViewPos timelineControllerPlayViewPos, IActuatorsService actuatorsService, TimelinePlayer timelinePlayer, ITimelineController[] timelineControllers, TimelineViewContext viewContext, PlayPosSynchronizer playPosSynchronizer)
        {
            _timelineData = timelineData;
            _timelineData.OnContentChanged += TimelineData_ContentChanged;
            _timelinePlayer = timelinePlayer;

            _actuatorsService = actuatorsService;
            _allActuators = _actuatorsService.AllActuators;
            _controllerTuneableActuators = _allActuators.Where(a => a.IsControllerTuneable).ToArray();

            _timelineControllers = timelineControllers;
            _timelineControllerPlayViewPos = timelineControllerPlayViewPos;

            _timelineEditingManipulation = new TimelineEditingManipulation(timelineData, playPosSynchronizer);

            _playPosSynchronizer = playPosSynchronizer;
            _playPosSynchronizer.OnPlayPosChanged += PlayPos_Changed;

            _viewContext = viewContext;
            _viewContext.Changed += ViewContext_Changed;

            foreach (var timelineController in _timelineControllers)
            {
                timelineController.OnTimelineEvent += TimelineController_OnTimelineEvent;
            }
        }

        private async void TimelineData_ContentChanged(object? sender, TimelineDataChangedEventArgs e)
        {
            switch (e.ChangeType)
            {
                case TimelineDataChangedEventArgs.ChangeTypes.NestedTimelinePointChanged:
                case TimelineDataChangedEventArgs.ChangeTypes.SoundPointChanged:
                case TimelineDataChangedEventArgs.ChangeTypes.ServoPointChanged:
                    if (!_manualUpdatingValuesViaController)
                    {
                        await _timelinePlayer.UpdateActuators();
                        ShowActuatorValuesOnTimelineInputController();
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"{nameof(e.ChangeType)}:{e.ChangeType}");    

            }   
        }

        private void PlayPos_Changed(object? sender, int newPlayPosMs)
        {
            if (!_manualUpdatingValuesViaController)
                ShowActuatorValuesOnTimelineInputController();

            _timelineControllerPlayViewPos.SetPlayPosFromTimelineControl(newPlayPosMs);
        }

        private void ViewContext_Changed(object? sender, ViewContextChangedEventArgs e)
        {
            switch (e.ChangeType)
            {
                case ViewContextChangedEventArgs.ChangeTypes.Duration:
                case ViewContextChangedEventArgs.ChangeTypes.PixelPerMs:
                case ViewContextChangedEventArgs.ChangeTypes.BankIndex:
                case ViewContextChangedEventArgs.ChangeTypes.FocusObject:
                    break;

                case ViewContextChangedEventArgs.ChangeTypes.FocusObjectValue:
                    if (_viewContext?.ActualFocusObject is IServo servo)
                        _timelineEditingManipulation.UpdateServoValue(servo, servo.PercentCalculator.CalculatePercent(servo.TargetValue));
                    if (_viewContext?.ActualFocusObject is ISoundPlayer soundPlayer)
                        _timelineEditingManipulation.UpdateSoundPlayerValue(soundPlayer, soundPlayer.ActualSoundId);
                    break;

                case ViewContextChangedEventArgs.ChangeTypes.Scroll:
                    break;

                default:
                    throw new ArgumentOutOfRangeException($"{nameof(e.ChangeType)}:{e.ChangeType}");
            }
        }


        /// <summary>
        /// Handle the events from the hardware timeline controllers like behringer x-touch mini or other midi controllers
        /// </summary>
        private async void TimelineController_OnTimelineEvent(object? sender, TimelineControllerEventArgs e)
        {
            if (_timelineData == null) return;

            // get the actuator referenced by the event
            IActuator? actuator = null;
            int actuatorIndexAbsolute = -1;
            if (e.ActuatorIndex_ != -1)
            {
                actuatorIndexAbsolute = e.ActuatorIndex_ + _viewContext.BankIndex * _viewContext.ItemsPerBank;
                if (actuatorIndexAbsolute < _controllerTuneableActuators.Length)
                    actuator = _controllerTuneableActuators[actuatorIndexAbsolute];
            }

            switch (e.EventType)
            {
                case TimelineControllerEventArgs.EventTypes.PlayPosAbsoluteChanged:
                    switch (_timelinePlayer.PlayState)
                    {
                        case TimelinePlayer.PlayStates.Playing:
                            break;

                        case TimelinePlayer.PlayStates.Nothing:
                            _timelineControllerPlayViewPos.SetPositionFromValueInPercent(e.ValueInPercent);
                            _playPosSynchronizer.SetNewPlayPos(_timelineControllerPlayViewPos.PlayPosAbsoluteMs);
                            break;

                        default:
                            throw new ArgumentOutOfRangeException($"{nameof(_timelinePlayer.PlayState)}:{_timelinePlayer.PlayState.ToString()}");
                    }
                    _lastActuatorChanged = -1;
                    break;

                case TimelineControllerEventArgs.EventTypes.NextBank:
                    SwitchToNextBank();
                    break;

                case TimelineControllerEventArgs.EventTypes.Play:
                    Play();
                    break;
                case TimelineControllerEventArgs.EventTypes.Stop:
                    Stop();
                    break;

                case TimelineControllerEventArgs.EventTypes.Forward:
                    if (_timelinePlayer.PlaybackSpeed < 4) _timelinePlayer.PlaybackSpeed += 0.5;
                    break;

                case TimelineControllerEventArgs.EventTypes.Backwards:
                    if (_timelinePlayer.PlaybackSpeed > 0.5) _timelinePlayer.PlaybackSpeed -= 0.5;
                    break;


                case TimelineControllerEventArgs.EventTypes.ActuatorValueChanged:
                    if (actuator != null)
                    {
                        var targetPercent = e.ValueInPercent;
                        switch (actuator)
                        {
                            case IServo servo:
                                _timelineEditingManipulation.UpdateServoValue(servo, targetPercent);
                                break;

                            case ISoundPlayer soundPlayer:
                                throw new NotImplementedException("todo: remove soundplayer from actuator bank counting!"); // todo: remove soundplayer from actuator bank counting!

                            default:
                                throw new ArgumentOutOfRangeException($"{nameof(actuator)}:{actuator} ");
                        }

                        if (_lastActuatorChanged != actuatorIndexAbsolute)
                        {
                            ShowActuatorValuesOnTimelineInputController();
                            _lastActuatorChanged = actuatorIndexAbsolute;
                        }
                    }
                    break;

                case TimelineControllerEventArgs.EventTypes.ActuatorSetValueToDefault:

                    if (actuator != null)
                    {
                        _lastActuatorChanged = -1;
                        switch (actuator)
                        {
                            case IServo servo:
                                _timelineEditingManipulation.UpdateServoValue(servo, servo.PercentCalculator.CalculatePercent(servo.DefaultValue));
                                break;

                            default:
                                throw new ArgumentOutOfRangeException($"{actuator.Id}/{actuator.Title} is an unhandled actutuator type.");
                        }

                        _manualUpdatingValuesViaController = true;
                        if (_manualUpdatingValuesViaController) await _timelinePlayer.UpdateActuators();
                        _manualUpdatingValuesViaController = false;
                    }
                    break;


                case TimelineControllerEventArgs.EventTypes.ActuatorTogglePoint:
                    _lastActuatorChanged = -1;
                    if (actuator != null)
                    {
                        switch (actuator)
                        {
                            case IServo servo:
                                _timelineEditingManipulation.ToggleServoPoint(servo, servo.PercentCalculator.CalculatePercent(servo.TargetValue));
                                break;

                            default:
                                throw new ArgumentOutOfRangeException($"{actuator.Id}/{actuator.Title} is an unhandled actutuator type.");
                        }

                        _manualUpdatingValuesViaController = true;
                        if (_manualUpdatingValuesViaController) await _timelinePlayer.UpdateActuators();
                        _manualUpdatingValuesViaController = false;
                    }
                    break;

                case TimelineControllerEventArgs.EventTypes.NextPage:
                    _lastActuatorChanged = -1;
                    break;

                case TimelineControllerEventArgs.EventTypes.PreviousPage:
                    _lastActuatorChanged = -1;
                    break;

                case TimelineControllerEventArgs.EventTypes.Save:
                    _lastActuatorChanged = -1;
                    break;

                default:
                    throw new ArgumentOutOfRangeException($"{nameof(e.EventType)}:{e.EventType.ToString()}");
            }
        }



        public void Play()
        {
            _timelinePlayer?.Play();
            foreach (var timelineController in _timelineControllers)
                timelineController?.SetPlayState(ITimelineController.PlayStates.Playing);
        }

        public async void Stop()
        {
            // snap scrollpos to snap positions 
            _timelinePlayer?.Stop();
            foreach (var timelineController in _timelineControllers)
                timelineController?.SetPlayState(ITimelineController.PlayStates.Editor);
        }

        private void SwitchToNextBank()
        {
            if (_viewContext == null) return;
            var newBankIndex = _viewContext.BankIndex + 1;
            var maxBankIndex = _actuatorsService.AllIds.Length / _viewContext.ItemsPerBank;
            if (newBankIndex > maxBankIndex) newBankIndex = 0;
            _viewContext.BankIndex = newBankIndex;
        }

        private void ShowActuatorValuesOnTimelineInputController()
        {
            if (_timelineControllers == null) return;

            var playPosMs = _playPosSynchronizer.PlayPosMs;

            if (_controllerTuneableActuators == null) return;

            for (int iActuator = 0; iActuator < _controllerTuneableActuators.Length; iActuator++)
            {
                var timelineControllerIndex = iActuator - _viewContext.BankIndex * _viewContext.ItemsPerBank;
                if (timelineControllerIndex < 0 || timelineControllerIndex >= _viewContext.ItemsPerBank) continue;

                switch (_controllerTuneableActuators[iActuator])
                {
                    case IServo servo:
                        foreach (var timelineController in _timelineControllers)
                        {
                            timelineController.SetActuatorValue(index: timelineControllerIndex, valueInPercent: Math.Max(0, Math.Min(100.0, 100.0 * (servo.TargetValue - servo.MinValue * 1.0) / (1.0 * servo.MaxValue - servo.MinValue))));
                            timelineController.ShowPointButtonState(index: timelineControllerIndex, pointExists: _timelineData.ServoPoints.Any(p => p.ServoId == servo.Id && p.TimeMs == playPosMs));
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"{_controllerTuneableActuators[iActuator].Id}/{_controllerTuneableActuators[iActuator].Title} is an unhandled actutuator type.");
                }
            }
        }



     


        public void Dispose()
        {

            _playPosSynchronizer.OnPlayPosChanged -= PlayPos_Changed;
            _timelineData.OnContentChanged -= TimelineData_ContentChanged;

            foreach (var timelineController in _timelineControllers)
            {
                timelineController.OnTimelineEvent -= TimelineController_OnTimelineEvent;
            }
        }
    }

}


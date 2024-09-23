// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Actuators;
using Awb.Core.InputControllers.TimelineInputControllers;
using Awb.Core.Player;
using Awb.Core.Project.Various;
using Awb.Core.Services;
using Awb.Core.Timelines;
using System;
using System.Diagnostics;
using System.Linq;

namespace AwbStudio.TimelineEditing
{
    internal class TimelineEventHandling : IDisposable
    {
        private readonly IAwbLogger _awbLogger;
        private readonly TimelineData _timelineData;
        private readonly TimelinePlayer _timelinePlayer;
        private readonly IActuatorsService _actuatorsService;
        private readonly PlayPosSynchronizer _playPosSynchronizer;
        private readonly ITimelineController[] _timelineControllers;
        private readonly TimelineControllerPlayViewPos _timelineControllerPlayViewPos;
        private readonly TimelineViewContext _viewContext;
        
        private readonly IActuator[] _allActuators;
        private readonly IActuator[] _controllerTuneableActuators;

        private readonly bool _writeDebug = false;

        private volatile int _timelineData_ContentChangedCounter = 0;
        private volatile int _timlineControllerUpdateCounter = 0;
        private volatile bool _updatingByTimelineController = false;

        public readonly TimelineEditingManipulation TimelineEditingManipulation;


        public TimelineEventHandling(
            TimelineData timelineData,
            TimelineControllerPlayViewPos timelineControllerPlayViewPos,
            IActuatorsService actuatorsService,
            TimelinePlayer timelinePlayer,
            ITimelineController[] timelineControllers,
            TimelineViewContext viewContext,
            PlayPosSynchronizer playPosSynchronizer,
            IAwbLogger awbLogger)
        {
            _awbLogger = awbLogger;

            _timelineData = timelineData;
            _timelineData.OnContentChanged += TimelineData_ContentChanged;
            _timelinePlayer = timelinePlayer;

            _actuatorsService = actuatorsService;
            _allActuators = _actuatorsService.AllActuators;
            _controllerTuneableActuators = _allActuators.Where(a => a.IsControllerTuneable).ToArray();

            _timelineControllers = timelineControllers;
            _timelineControllerPlayViewPos = timelineControllerPlayViewPos;

            TimelineEditingManipulation = new TimelineEditingManipulation(timelineData, playPosSynchronizer);

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
            var myCounter = ++_timelineData_ContentChangedCounter;

            if (_writeDebug) Debug.WriteLine($"TimelineData_ContentChanged: {e.ChangeType} START ");
            switch (e.ChangeType)
            {
                case TimelineDataChangedEventArgs.ChangeTypes.NestedTimelinePointChanged:
                case TimelineDataChangedEventArgs.ChangeTypes.SoundPointChanged:
                case TimelineDataChangedEventArgs.ChangeTypes.ServoPointChanged:
                case TimelineDataChangedEventArgs.ChangeTypes.CopyNPaste:
                    await _timelinePlayer.RequestActuatorUpdate();
                    if (!_updatingByTimelineController && myCounter  == _timelineData_ContentChangedCounter)
                    {
                        if (sender is ITimelineController timelineController)
                            ShowActuatorValuesOnTimelineInputController(dontUpdateThisController: timelineController);
                        else
                            ShowActuatorValuesOnTimelineInputController(dontUpdateThisController: null);
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException($"{nameof(e.ChangeType)}:{e.ChangeType}");
            }
            if (_writeDebug) Debug.WriteLine($"TimelineData_ContentChanged: {e.ChangeType} DONE");
        }

        private void PlayPos_Changed(object? sender, int newPlayPosMs)
        {
            if (_writeDebug) Debug.WriteLine($"PlayPos_Changed: {newPlayPosMs}");
            if (sender is ITimelineController timelineController)
                ShowActuatorValuesOnTimelineInputController(dontUpdateThisController: timelineController);
            else
                ShowActuatorValuesOnTimelineInputController(dontUpdateThisController: null);

            _timelineControllerPlayViewPos.SetPlayPosFromTimelineControl(newPlayPosMs);
        }

        private void ViewContext_Changed(object? sender, ViewContextChangedEventArgs e)
        {
            if (_writeDebug) Debug.WriteLine($"ViewContext_Changed: {e.ChangeType}");
            switch (e.ChangeType)
            {
                case ViewContextChangedEventArgs.ChangeTypes.Duration:
                case ViewContextChangedEventArgs.ChangeTypes.PixelPerMs:
                case ViewContextChangedEventArgs.ChangeTypes.BankIndex:
                case ViewContextChangedEventArgs.ChangeTypes.FocusObject:
                case ViewContextChangedEventArgs.ChangeTypes.Scroll:
                case ViewContextChangedEventArgs.ChangeTypes.Selection:
                    break;

                case ViewContextChangedEventArgs.ChangeTypes.FocusObjectValue:
                    if (_viewContext?.ActualFocusObject is IServo servo)
                        TimelineEditingManipulation.UpdateServoValue(servo, servo.PercentCalculator.CalculatePercent(servo.TargetValue));
                    if (_viewContext?.ActualFocusObject is ISoundPlayer soundPlayer)
                        TimelineEditingManipulation.UpdateSoundPlayerValue(soundPlayer, soundPlayer.ActualSoundId, soundTitle: null);
                    if (_viewContext?.ActualFocusObject == NestedTimelinesFakeObject.Singleton)
                        TimelineEditingManipulation.UpdateNestedTimelinesValue();
                    break;

                default:
                    throw new ArgumentOutOfRangeException($"{nameof(e.ChangeType)}:{e.ChangeType}");
            }
        }


        /// <summary>
        /// Handle the events from the hardware timeline controllers like behringer x-touch mini or other midi controllers
        /// </summary>
        private async void TimelineController_OnTimelineEvent(object? senderController, TimelineControllerEventArgs e)
        {
            if (_timelineData == null) return;
            _updatingByTimelineController = true;

            var myUpdateCount = ++_timlineControllerUpdateCounter;

            if (_writeDebug) Debug.WriteLine($"TimelineController_OnTimelineEvent: {e.EventType}, {e.ValueInPercent} START " + myUpdateCount);

            ITimelineController? timelineController = (ITimelineController?)senderController;
            if (senderController != null && timelineController == null)
            {
                await _awbLogger.LogErrorAsync($"TimelineController_OnTimelineEvent: senderController is not null but cannot be casted to ITimelineController");
                return;
            }

            // get the actuator referenced by the event
            IActuator? actuator = null;
            if (e.ActuatorIndex_ != -1)
            {
                int actuatorIndexAbsolute = e.ActuatorIndex_ + _viewContext.BankIndex * _viewContext.ItemsPerBank;
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
                                TimelineEditingManipulation.UpdateServoValue(servo, targetPercent);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException($"{nameof(actuator)}:{actuator} ");
                        }
                        if (myUpdateCount == _timlineControllerUpdateCounter) // no other update has been started in the meantime
                            await _timelinePlayer.RequestActuatorUpdate();
                        ShowActuatorValuesOnTimelineInputController(timelineController);

                    }
                    break;

                case TimelineControllerEventArgs.EventTypes.ActuatorSetValueToDefault:

                    if (actuator != null)
                    {
                        switch (actuator)
                        {
                            case IServo servo:
                                TimelineEditingManipulation.UpdateServoValue(servo, servo.PercentCalculator.CalculatePercent(servo.DefaultValue));
                                break;

                            default:
                                throw new ArgumentOutOfRangeException($"{actuator.Id}/{actuator.Title} is an unhandled actutuator type.");
                        }
                        if (myUpdateCount == _timlineControllerUpdateCounter) // no other update has been started in the meantime
                            await _timelinePlayer.RequestActuatorUpdate();
                    }
                    break;


                case TimelineControllerEventArgs.EventTypes.ActuatorTogglePoint:
                    if (actuator != null)
                    {
                        switch (actuator)
                        {
                            case IServo servo:
                                TimelineEditingManipulation.ToggleServoPoint(servo);
                                break;

                            default:
                                throw new ArgumentOutOfRangeException($"{actuator.Id}/{actuator.Title} is an unhandled actutuator type.");
                        }
                        if (myUpdateCount == _timlineControllerUpdateCounter) // no other update has been started in the meantime
                            await _timelinePlayer.RequestActuatorUpdate();
                    }
                    break;

                case TimelineControllerEventArgs.EventTypes.NextPage:
                case TimelineControllerEventArgs.EventTypes.PreviousPage:
                case TimelineControllerEventArgs.EventTypes.Save:
                    break;

                default:
                    throw new ArgumentOutOfRangeException($"{nameof(e.EventType)}:{e.EventType.ToString()}");
            }
            _updatingByTimelineController = false;
            if (_writeDebug) Debug.WriteLine($"TimelineController_OnTimelineEvent: {e.EventType}, {e.ValueInPercent} END " + myUpdateCount);
        }

        public async void Play()
        {
            _timelinePlayer?.Play();
            foreach (var timelineController in _timelineControllers)
                await timelineController.SetPlayStateAsync(ITimelineController.PlayStates.Playing);
        }

        public async void Stop()
        {
            // snap scrollpos to snap positions 
            _timelinePlayer?.Stop();
            foreach (var timelineController in _timelineControllers)
                await timelineController.SetPlayStateAsync(ITimelineController.PlayStates.Editor);
        }

        private void SwitchToNextBank()
        {
            if (_viewContext == null) return;
            var newBankIndex = _viewContext.BankIndex + 1;
            var maxBankIndex = _actuatorsService.AllIds.Length / _viewContext.ItemsPerBank;
            if (newBankIndex > maxBankIndex) newBankIndex = 0;
            _viewContext.BankIndex = newBankIndex;
        }

        private void ShowActuatorValuesOnTimelineInputController(ITimelineController? dontUpdateThisController)
        {
            if (_timelineControllers == null) return;
            if (_controllerTuneableActuators == null) return;

            if (_writeDebug) Debug.WriteLine($"ShowActuatorValuesOnTimelineInputController");

            var playPosMs = _playPosSynchronizer.PlayPosMsGuaranteedSnapped;

            for (int iActuator = 0; iActuator < _controllerTuneableActuators.Length; iActuator++)
            {
                var timelineControllerIndex = iActuator - _viewContext.BankIndex * _viewContext.ItemsPerBank;
                if (timelineControllerIndex < 0 || timelineControllerIndex >= _viewContext.ItemsPerBank) continue;

                switch (_controllerTuneableActuators[iActuator])
                {
                    case IServo servo:
                        foreach (var timelineController in _timelineControllers)
                        {
                            if (dontUpdateThisController == timelineController)
                            {
                                continue;
                            }
                            timelineController.SetActuatorValueAsync(index: timelineControllerIndex, valueInPercent: servo.PercentCalculator.CalculatePercent(servo.TargetValue));
                            timelineController.ShowPointButtonStateAsync(index: timelineControllerIndex, pointExists: _timelineData.ServoPoints.Any(p => p.ServoId == servo.Id && p.TimeMs == playPosMs));
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


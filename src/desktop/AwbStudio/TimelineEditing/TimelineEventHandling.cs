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
        private volatile bool _manualUpdatingValuesViaController;
        private int _lastActuatorChanged = 1; // prevent double actuator change events to the midi controller

        public TimelineEventHandling(TimelineData timelineData, TimelineControllerPlayViewPos timelineControllerPlayViewPos, IActuatorsService actuatorsService, TimelinePlayer timelinePlayer, ITimelineController[] timelineControllers, TimelineViewContext viewContext, PlayPosSynchronizer playPosSynchronizer)
        {
            _timelineData = timelineData;
            _timelinePlayer = timelinePlayer;

            _actuatorsService = actuatorsService;
            _allActuators = _actuatorsService.AllActuators;

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



        private async void TimelineController_OnTimelineEvent(object? sender, TimelineControllerEventArgs e)
        {
            if (_timelineData == null) return;

            // get the actuator referenced by the event
            IActuator? actuator = null;
            int actuatorIndexAbsolute = -1;
            if (e.ActuatorIndex_ != -1)
            {
                actuatorIndexAbsolute = e.ActuatorIndex_ + _viewContext.BankIndex * _viewContext.ItemsPerBank;
                if (actuatorIndexAbsolute < _allActuators.Length)
                    actuator = _allActuators[actuatorIndexAbsolute];
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
                                var servoPoint = _timelineData?.ServoPoints.OfType<ServoPoint>().SingleOrDefault(p => p.ServoId == servo.Id && (int)p.TimeMs == _playPosSynchronizer.PlayPosMs); // check existing point
                                if (servoPoint == null)
                                {
                                    // Insert a new servo point
                                    var targetValue = servo.DefaultValue;
                                    var targetPercent = servo.PercentCalculator.CalculatePercent(targetValue);
                                    servo.TargetValue = targetValue;
                                    servoPoint = new ServoPoint(servo.Id, targetPercent, _playPosSynchronizer.PlayPosMs);
                                    _timelineData?.ServoPoints.Add(servoPoint);
                                }
                                else
                                {
                                    // set target value to default
                                    var targetValue = servo.DefaultValue;
                                    var targetPercent = servo.PercentCalculator.CalculatePercent(targetValue);
                                    servoPoint.ValuePercent = targetPercent;
                                    servo.TargetValue = targetValue;
                                }
                                _timelineData!.SetContentChanged(TimelineDataChangedEventArgs.ChangeTypes.ServoPointChanged, servoPoint.ServoId);
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
                                var servoPoint = _timelineData?.ServoPoints.OfType<ServoPoint>().SingleOrDefault(p => p.ServoId == servo.Id && (int)p.TimeMs == _playPosSynchronizer.PlayPosMs); // check existing point
                                if (servoPoint == null)
                                {
                                    // Insert a new servo point
                                    var targetValue = servo.TargetValue;
                                    var targetPercent = servo.PercentCalculator.CalculatePercent(targetValue);
                                    servo.TargetValue = targetValue;
                                    servoPoint = new ServoPoint(servo.Id, targetPercent, _playPosSynchronizer.PlayPosMs);
                                    _timelineData?.ServoPoints.Add(servoPoint);
                                }
                                else
                                {
                                    // Remove the existing servo point
                                    _timelineData?.ServoPoints.Remove(servoPoint);
                                }
                                _timelineData!.SetContentChanged(TimelineDataChangedEventArgs.ChangeTypes.ServoPointChanged, servoPoint.ServoId);
                                break;

                            case ISoundPlayer soundPlayer:
                                throw new NotImplementedException("todo: remove soundplayer from actuator bank counting!"); // todo: remove soundplayer from actuator bank counting!
                                /*  if (_project.Sounds?.Any() == true)
                                  {

                                      var soundPoint = _timelineData?.SoundPoints.OfType<SoundPoint>().SingleOrDefault(p => p.SoundPlayerId == soundPlayer.Id && (int)p.TimeMs == _playPosSynchronizer.PlayPosMs); // check existing point
                                      if (soundPoint == null)
                                      {
                                          // Insert a new sound point
                                          var soundId = soundPlayer.ActualSoundId == 0 ? _project.Sounds.FirstOrDefault()?.Id : soundPlayer.ActualSoundId;
                                          var sound = _project.Sounds.FirstOrDefault(s => s.Id == soundId);
                                          if (sound == null)
                                          {
                                              MessageBox.Show($"Actual sound id{soundPlayer.ActualSoundId} not found");
                                          }
                                          else
                                          {
                                              soundPoint = new SoundPoint(timeMs: _playPosSynchronizer.PlayPosMs, soundPlayerId: soundPlayer.Id, title: sound.Title, soundId: soundPlayer.ActualSoundId);
                                              _timelineData?.SoundPoints.Add(soundPoint);
                                          }
                                      }
                                      else
                                      {
                                          // Remove the existing sound point
                                          _timelineData?.SoundPoints.Remove(soundPoint);
                                      }
                                      _timelineData!.SetContentChanged(TimelineDataChangedEventArgs.ChangeTypes.SoundPointChanged, soundPlayer.Id);
                                  }*/
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
                    var servo = _viewContext.ActualFocusObject as IServo;
                    if (servo != null)
                    {
                        _timelineEditingManipulation.UpdateServoValue(servo, servo.PercentCalculator.CalculatePercent(servo.TargetValue));
                    }
                    break;

                case ViewContextChangedEventArgs.ChangeTypes.Scroll:
                    break;

                default:
                    throw new ArgumentOutOfRangeException($"{nameof(e.ChangeType)}:{e.ChangeType}");
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

            var actuators = _actuatorsService?.AllActuators;
            if (actuators == null) return;

            for (int iActuator = 0; iActuator < actuators.Length; iActuator++)
            {
                var timelineControllerIndex = iActuator - _viewContext.BankIndex * _viewContext.ItemsPerBank;
                if (timelineControllerIndex < 0 || timelineControllerIndex >= _viewContext.ItemsPerBank) continue;

                switch (actuators[iActuator])
                {
                    case IServo servo:
                        foreach (var timelineController in _timelineControllers)
                        {
                            timelineController.SetActuatorValue(index: timelineControllerIndex, valueInPercent: Math.Max(0, Math.Min(100.0, 100.0 * (servo.TargetValue - servo.MinValue * 1.0) / (1.0 * servo.MaxValue - servo.MinValue))));
                            timelineController.ShowPointButtonState(index: timelineControllerIndex, pointExists: _timelineData.ServoPoints.Any(p => p.ServoId == servo.Id && p.TimeMs == playPosMs));
                        }
                        break;
                    case ISoundPlayer soundPlayer:
                        throw new NotImplementedException("todo: remove soundplayer from actuator bank counting!"); // todo: remove soundplayer from actuator bank counting!
                        /* foreach (var timelineController in _timelineControllers)
                         {
                             if (_project.Sounds?.Any() == true)
                             {
                                 var soundIndex = -1;
                                 for (int iSnd = 0; iSnd < _project.Sounds.Length; iSnd++)
                                 {
                                     if (_project.Sounds[iSnd].Id == soundPlayer.ActualSoundId)
                                     {
                                         soundIndex = iSnd;
                                         break;
                                     }
                                 }
                                 if (soundIndex != -1)
                                 {
                                     timelineController.SetActuatorValue(index: timelineControllerIndex, valueInPercent: Math.Max(0, Math.Min(100.0, 100.0 * soundIndex / _project.Sounds.Length)));
                                     timelineController.ShowPointButtonState(index: timelineControllerIndex, pointExists: _timelineData.SoundPoints.Any(p => p.SoundPlayerId == soundPlayer.Id && p.TimeMs == playPosMs));
                                 }
                             }
                         }*/
                    default:
                        throw new ArgumentOutOfRangeException($"{actuators[iActuator].Id}/{actuators[iActuator].Title} is an unhandled actutuator type.");
                }
            }
        }



        private void PlayPos_Changed(object? sender, int newPlayPosMs)
        {
            if (!_manualUpdatingValuesViaController)
                ShowActuatorValuesOnTimelineInputController();

            _timelineControllerPlayViewPos.SetPlayPosFromTimelineControl(newPlayPosMs);
        }


        public void Dispose()
        {

            _playPosSynchronizer.OnPlayPosChanged -= PlayPos_Changed;

            foreach (var timelineController in _timelineControllers)
            {
                timelineController.OnTimelineEvent -= TimelineController_OnTimelineEvent;
            }
        }
    }

}


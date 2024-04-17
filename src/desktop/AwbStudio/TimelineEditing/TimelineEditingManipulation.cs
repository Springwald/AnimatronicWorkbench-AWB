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
    internal class TimelineEditingManipulation : IDisposable
    {
        private readonly TimelineData _timelineData;
        private readonly TimelinePlayer _timelinePlayer;
        private readonly IActuatorsService _actuatorsService;
        private readonly PlayPosSynchronizer _playPosSynchronizer;
        private readonly ITimelineController[] _timelineControllers;
        private readonly TimelineControllerPlayViewPos _timelineControllerPlayViewPos;
        private readonly TimelineViewContext? _viewContext;
        private volatile bool _manualUpdatingValues;
        private int _lastActuatorChanged = 1; // prevent double actuator change events to the midi controller

        public TimelineEditingManipulation(TimelineData timelineData, TimelineControllerPlayViewPos timelineControllerPlayViewPos, IActuatorsService actuatorsService, TimelinePlayer timelinePlayer, ITimelineController[] timelineControllers, TimelineViewContext? viewContext, PlayPosSynchronizer playPosSynchronizer)
        {
            _timelineData = timelineData;
            _timelinePlayer = timelinePlayer;
            _actuatorsService = actuatorsService;
            _playPosSynchronizer = playPosSynchronizer;
            _timelineControllers = timelineControllers;
            _timelineControllerPlayViewPos = timelineControllerPlayViewPos;
            _viewContext = viewContext;

            _playPosSynchronizer.OnPlayPosChanged += PlayPos_Changed;

            foreach (var timelineController in _timelineControllers)
            {
                timelineController.OnTimelineEvent += TimelineController_OnTimelineEvent;
            }

        }

        private async void TimelineController_OnTimelineEvent(object? sender, TimelineControllerEventArgs e)
        {
            if (_timelineData == null) return;

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

                    var allActuators = _actuatorsService?.AllActuators;
                    if (allActuators == null) return;

                    var actuatorIndex = e.ActuatorIndex_ + _viewContext.BankIndex * _viewContext.ItemsPerBank;
                    if (actuatorIndex >= allActuators.Length) return;

                    var actuator = allActuators[actuatorIndex];
                    var targetPercent = e.ValueInPercent;

                    switch (actuator)
                    {
                        case IServo servo:
                            this.UpdateServoValue(servo, targetPercent);
                            break;

                        case ISoundPlayer soundPlayer:
                            /*    if (_project.Sounds.Length > 0)
                                {
                                    var soundIndex = (int)((targetPercent * (_project.Sounds.Length - 1)) / 100);
                                    if (soundIndex > 0 && soundIndex < _project.Sounds.Length)
                                    {
                                        var sound = _project.Sounds[soundIndex];
                                        var soundPoint = _timelineData?.SoundPoints.OfType<SoundPoint>().SingleOrDefault(p => p.SoundPlayerId == soundPlayer.Id && (int)p.TimeMs == _playPosSynchronizer.PlayPosMs); // check existing point
                                        if (soundPoint == null)
                                        {
                                            soundPoint = new SoundPoint(timeMs: _playPosSynchronizer.PlayPosMs, soundPlayerId: soundPlayer.Id, title: sound.Title, soundId: sound.Id); ;
                                            _timelineData!.SoundPoints.Add(soundPoint);
                                        }
                                        else
                                        {
                                            soundPoint.SoundId = sound.Id;
                                            soundPoint.Title = sound.Title;
                                        }
                                        _timelineData!.SetContentChanged(TimelineDataChangedEventArgs.ChangeTypes.SoundPointChanged, soundPoint.SoundPlayerId);

                                    }
                                }*/
                            break;

                        default:
                            throw new ArgumentOutOfRangeException($"{nameof(actuator)}:{actuator} ");
                    }

                    if (_lastActuatorChanged != actuatorIndex)
                    {
                        ShowActuatorValuesOnTimelineInputController();
                        _lastActuatorChanged = actuatorIndex;
                    }
                    break;

                case TimelineControllerEventArgs.EventTypes.ActuatorSetValueToDefault:
                case TimelineControllerEventArgs.EventTypes.ActuatorTogglePoint:
                    _lastActuatorChanged = -1;
                    var actuators = _actuatorsService?.AllActuators;
                    if (actuators == null) return;

                    actuatorIndex = e.ActuatorIndex_ + _viewContext.BankIndex * _viewContext.ItemsPerBank;
                    if (actuatorIndex >= actuators.Length) return;

                    actuator = actuators[actuatorIndex];

                    switch (actuator)
                    {
                        case IServo servo:
                            var servoPoint = _timelineData?.ServoPoints.OfType<ServoPoint>().SingleOrDefault(p => p.ServoId == servo.Id && (int)p.TimeMs == _playPosSynchronizer.PlayPosMs); // check existing point
                            if (servoPoint == null)
                            {
                                // Insert a new servo point
                                var targetValue = e.EventType == TimelineControllerEventArgs.EventTypes.ActuatorSetValueToDefault ? servo.DefaultValue : servo.TargetValue;
                                targetPercent = 100.0 * (targetValue - servo.MinValue) / (servo.MaxValue - servo.MinValue);
                                servo.TargetValue = targetValue;
                                servoPoint = new ServoPoint(servo.Id, targetPercent, _playPosSynchronizer.PlayPosMs);
                                _timelineData?.ServoPoints.Add(servoPoint);
                            }
                            else
                            {

                                if (e.EventType == TimelineControllerEventArgs.EventTypes.ActuatorSetValueToDefault)
                                {
                                    // set target value to default
                                    var targetValue = servo.DefaultValue;
                                    targetPercent = 100.0 * (targetValue - servo.MinValue) / (servo.MaxValue - servo.MinValue);
                                    servoPoint.ValuePercent = targetPercent;
                                    servo.TargetValue = targetValue;
                                }
                                else
                                {
                                    // Remove the existing servo point
                                    _timelineData?.ServoPoints.Remove(servoPoint);
                                }
                            }
                            _timelineData!.SetContentChanged(TimelineDataChangedEventArgs.ChangeTypes.ServoPointChanged, servoPoint.ServoId);
                            break;
                        case ISoundPlayer soundPlayer:
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

                    _manualUpdatingValues = true;
                    if (_manualUpdatingValues) await _timelinePlayer.UpdateActuators();
                    _manualUpdatingValues = false;
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


        public void UpdateServoValue(IServo servo, double targetPercent)
        {
            var servoPoint = _timelineData?.ServoPoints.OfType<ServoPoint>().SingleOrDefault(p => p.ServoId == servo.Id && (int)p.TimeMs == _playPosSynchronizer.PlayPosMs); // check existing point
            if (servoPoint == null)
            {
                servoPoint = new ServoPoint(servo.Id, targetPercent, _playPosSynchronizer.PlayPosMs);
                _timelineData?.ServoPoints.Add(servoPoint);
            }
            else
            {
                servoPoint.ValuePercent = targetPercent;
            }
            _timelineData!.SetContentChanged(TimelineDataChangedEventArgs.ChangeTypes.ServoPointChanged, servoPoint.ServoId);
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
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"{actuators[iActuator].Id}/{actuators[iActuator].Title} is an unhandled actutuator type.");
                }
            }
        }



        private void PlayPos_Changed(object? sender, int newPlayPosMs)
        {
            if (!_manualUpdatingValues)
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

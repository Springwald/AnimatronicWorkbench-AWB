// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Actuators;
using Awb.Core.Player;
using Awb.Core.Timelines;
using System.Linq;

namespace AwbStudio.TimelineEditing
{
    public class TimelineEditingManipulation
    {
        private readonly TimelineData _timelineData;
        private readonly PlayPosSynchronizer _playPosSynchronizer;

        public TimelineEditingManipulation(TimelineData timelineData, PlayPosSynchronizer playPosSynchronizer)
        {
            _timelineData = timelineData;
            _playPosSynchronizer = playPosSynchronizer;
        }

        #region SERVOS

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

        public void ToggleServoPoint(IServo servo, double percentValue)
        {
            var servoPoint = _timelineData?.ServoPoints.OfType<ServoPoint>().SingleOrDefault(p => p.ServoId == servo.Id && (int)p.TimeMs == _playPosSynchronizer.PlayPosMs); // check existing point
            if (servoPoint == null)
            {
                // Insert a new servo point
                servo.TargetValue = (int)servo.PercentCalculator.CalculateValue(percentValue);
                servoPoint = new ServoPoint(servo.Id, percentValue, _playPosSynchronizer.PlayPosMs);
                _timelineData?.ServoPoints.Add(servoPoint);
            }
            else
            {
                // Remove the existing servo point
                _timelineData?.ServoPoints.Remove(servoPoint);
            }
            _timelineData!.SetContentChanged(TimelineDataChangedEventArgs.ChangeTypes.ServoPointChanged, servoPoint.ServoId);
        }

        #endregion

        #region SOUNDPLAYER

        public void UpdateSoundPlayerValue(ISoundPlayer soundPlayer, int soundId)
        {
            var soundPoint = _timelineData?.SoundPoints.OfType<SoundPoint>().SingleOrDefault(p => p.SoundPlayerId == soundPlayer.Id && (int)p.TimeMs == _playPosSynchronizer.PlayPosMs); // check existing point
            if (soundPoint == null)
            {
                soundPoint = new SoundPoint(_playPosSynchronizer.PlayPosMs, soundPlayer.Id, "Sound " + soundId, soundId);
                _timelineData?.SoundPoints.Add(soundPoint);
            }
            else
            {
                soundPoint.SoundId = soundId;
            }
            _timelineData!.SetContentChanged(TimelineDataChangedEventArgs.ChangeTypes.SoundPointChanged, soundPlayer.Id);
        }

        public void RemoveSoundPoint(ISoundPlayer soundPlayer)
        {
            var soundPoint = _timelineData?.SoundPoints.OfType<SoundPoint>().SingleOrDefault(p => p.SoundPlayerId == soundPlayer.Id && (int)p.TimeMs == _playPosSynchronizer.PlayPosMs); // check existing point
            if (soundPoint == null) return;

            // Remove the existing sound point
            _timelineData?.SoundPoints.Remove(soundPoint);
            _timelineData!.SetContentChanged(TimelineDataChangedEventArgs.ChangeTypes.SoundPointChanged, soundPoint.SoundPlayerId);
        }

        #endregion
    }
}
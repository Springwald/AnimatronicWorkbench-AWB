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
            var point = _timelineData.GetPoint<ServoPoint>(_playPosSynchronizer.PlayPosMsGuaranteedSnapped, servo.Id);
            if (point == null)
            {
                point = new ServoPoint(servo.Id, targetPercent, _playPosSynchronizer.PlayPosMsGuaranteedSnapped);
                _timelineData?.ServoPoints.Add(point);
            }
            else
            {
                point.ValuePercent = targetPercent;
                _timelineData!.SetContentChanged(TimelineDataChangedEventArgs.ChangeTypes.ServoPointChanged, servo.Id);
            }   
        }

        public void ToggleServoPoint(IServo servo)
        {
            var servoPoint = _timelineData?.ServoPoints.OfType<ServoPoint>().SingleOrDefault(p => p.ServoId == servo.Id && (int)p.TimeMs == _playPosSynchronizer.PlayPosMsGuaranteedSnapped); // check existing point
            if (servoPoint == null)
            {
                // Insert a new servo point
                var percentValue = servo.PercentCalculator.CalculatePercent(servo.TargetValue);
                servoPoint = new ServoPoint(servo.Id, percentValue, _playPosSynchronizer.PlayPosMsGuaranteedSnapped);
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
            var soundPoint = _timelineData?.SoundPoints.OfType<SoundPoint>().SingleOrDefault(p => p.SoundPlayerId == soundPlayer.Id && (int)p.TimeMs == _playPosSynchronizer.PlayPosMsGuaranteedSnapped); // check existing point
            if (soundPoint == null)
            {
                soundPoint = new SoundPoint(_playPosSynchronizer.PlayPosMsGuaranteedSnapped, soundPlayer.Id, "Sound " + soundId, soundId);
                _timelineData?.SoundPoints.Add(soundPoint);
            }
            else
            {
                soundPoint.SoundId = soundId;
            }
            _timelineData!.SetContentChanged(TimelineDataChangedEventArgs.ChangeTypes.SoundPointChanged, soundPlayer.Id);
        }


        #endregion
    }
}
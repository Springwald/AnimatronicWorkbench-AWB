// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Actuators;
using Awb.Core.Player;
using Awb.Core.Project.Various;
using Awb.Core.Timelines;
using Awb.Core.Timelines.NestedTimelines;
using System.Linq;

namespace AwbStudio.TimelineEditing
{
    /// <summary>
    /// Routines to manipulate the timeline data
    /// </summary>
    public class TimelineEditingManipulation
    {
        private readonly TimelineData _timelineData;
        private readonly PlayPosSynchronizer _playPosSynchronizer;

        public TimelineEditingManipulation(TimelineData timelineData, PlayPosSynchronizer playPosSynchronizer)
        {
            _timelineData = timelineData;
            _playPosSynchronizer = playPosSynchronizer;
        }

        #region COPY + PASTE

        #endregion

        #region SERVOS

        public void UpdateServoValue(IServo servo, double targetPercent)
        {
            var point = _timelineData.ServoPoints.GetPoint<ServoPoint>(_playPosSynchronizer.PlayPosMsGuaranteedSnapped, servo.Id);
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
            var servoPoint = _timelineData?.ServoPoints.SingleOrDefault(p => p.ServoId == servo.Id && (int)p.TimeMs == _playPosSynchronizer.PlayPosMsGuaranteedSnapped); // check existing point
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

        public void UpdateSoundPlayerValue(ISoundPlayer soundPlayer, int? soundId, string? soundTitle)
        {
            var soundPoint = _timelineData?.SoundPoints.SingleOrDefault(p => p.SoundPlayerId == soundPlayer.Id && (int)p.TimeMs == _playPosSynchronizer.PlayPosMsGuaranteedSnapped); // check existing point
            if (soundId == null)
            {
                // sound should be removed from timeline at this position
                if (soundPoint == null) return; // nothing to do
                _timelineData?.SoundPoints.Remove(soundPoint);
            }
            else
            {
                // sound should be added or updated at this position
                if (soundPoint == null)
                {
                    soundPoint = new SoundPoint(_playPosSynchronizer.PlayPosMsGuaranteedSnapped, soundPlayer.Id, soundTitle ?? $"Sound {soundId}", soundId.Value);
                    _timelineData?.SoundPoints.Add(soundPoint);
                }
                else
                {
                    soundPoint.SoundId = soundId.Value;
                }
            }
            _timelineData!.SetContentChanged(TimelineDataChangedEventArgs.ChangeTypes.SoundPointChanged, soundPlayer.Id);
        }


        #endregion

        public void UpdateNestedTimelinesValue()
        {
            var timelineId = NestedTimelinesFakeObject.Singleton.ActualNestedTimelineId;
            var nestedTimelinePoint = _timelineData?.NestedTimelinePoints.SingleOrDefault(p => (int)p.TimeMs == _playPosSynchronizer.PlayPosMsGuaranteedSnapped); // check existing point
            if (timelineId == null)
            {
                // nested timeline value should be removed from timeline at this position
                if (nestedTimelinePoint == null) return; // nothing to do
                _timelineData?.NestedTimelinePoints.Remove(nestedTimelinePoint);
            }
            else
            {
                // nested timeline value should be added or updated at this position
                if (nestedTimelinePoint == null)
                {
                    nestedTimelinePoint = new NestedTimelinePoint(_playPosSynchronizer.PlayPosMsGuaranteedSnapped, timelineId);
                    _timelineData?.NestedTimelinePoints.Add(nestedTimelinePoint);
                }
                else
                {
                    nestedTimelinePoint.TimelineId = timelineId;
                }
            }
            _timelineData!.SetContentChanged(TimelineDataChangedEventArgs.ChangeTypes.NestedTimelinePointChanged, NestedTimelinesFakeObject.Singleton.Id);
        }
    }
}
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

        /// <summary>
        /// Copies the timeline content between the given start and end position into the copy buffer.
        /// </summary>
        /// <returns></returns>
        public CopyNPasteBuffer? Copy(int startMs, int endMs)
        {
            if (startMs == endMs) return null;
            if (startMs > endMs) (startMs, endMs) = (endMs, startMs); // swap values
            var points = _timelineData.AllPoints.Where(p => p.TimeMs >= startMs && p.TimeMs <= endMs).ToList();
            return new CopyNPasteBuffer { TimelinePoints = points, OldEndMs = endMs, OldStartMs = startMs };
            // todo: undo logic
        }

        /// <summary>
        /// Cuts the timeline content between the given start and end position into the copy buffer.
        /// Removes also the space between start and end position, so the timeline gets shorter by the duration of the cut content.
        /// </summary>
        public CopyNPasteBuffer? Cut(int startMs, int endMs)
        {
            if (startMs == endMs) return null;
            if (startMs > endMs) (startMs, endMs) = (endMs, startMs); // swap values
            var points = _timelineData.AllPoints.Where(p => p.TimeMs >= startMs && p.TimeMs <= endMs).ToList();
            foreach (var point in points)
                _ = _timelineData.RemovePoint(point);
            if (MovePoints(oldStartMs: endMs, oldEndMs: int.MaxValue, newStartMs: startMs) == false) return null; // todo: better error handling

            return new CopyNPasteBuffer { TimelinePoints = points, OldEndMs = endMs, OldStartMs = startMs };
            // todo: undo logic
        }

        /// <summary>
        /// Inserts the timeline content from the copy buffer into the timeline at the given position.
        /// The timeline gets longer by the duration of the copied content.
        /// </summary>
        public bool Paste(CopyNPasteBuffer buffer, int targetMs)
        {
            if (buffer == null) return false;
            return true;
            // todo: undo logic
        }

        /// <summary>
        /// Clears the timeline content between the given start and end position.
        /// Does not remove the space between the points, only the points themselves.
        /// </summary>
        public bool Clear(int startMs, int endMs)
        {
            return false;
            // todo: undo logic
        }

        /// <summary>
        /// moves the timeline points between the given start and end position to the new start position
        /// </summary>
        private bool MovePoints(int oldStartMs, int oldEndMs, int newStartMs)
        {
            var pointsBetween = _timelineData.AllPoints.Where(p => p.TimeMs >= oldStartMs && p.TimeMs <= oldEndMs).ToList();
            return false;
        }

        #endregion

        #region SERVOS

        /// <summary>
        /// sets a servo value at the current play position
        /// </summary>
        public void UpdateServoValue(IServo servo, double targetPercent)
        {
            var point = _timelineData.ServoPoints.GetPoint<ServoPoint>(_playPosSynchronizer.PlayPosMsGuaranteedSnapped, servo.Id);
            if (point == null)
            {
                point = new ServoPoint(servo.Id, targetPercent, _playPosSynchronizer.PlayPosMsGuaranteedSnapped);
                _ = _timelineData?.AddPoint(point);
            }
            else
            {
                point.ValuePercent = targetPercent;
                _timelineData!.SetContentChanged(TimelineDataChangedEventArgs.ChangeTypes.ServoPointChanged, servo.Id);
            }
            // todo: undo logic
        }

        /// <summary>
        /// inserts or removes a servo point at the current play position
        /// </summary>
        public void ToggleServoPoint(IServo servo)
        {
            var servoPoint = _timelineData?.ServoPoints.SingleOrDefault(p => p.ServoId == servo.Id && (int)p.TimeMs == _playPosSynchronizer.PlayPosMsGuaranteedSnapped); // check existing point
            if (servoPoint == null)
            {
                // Insert a new servo point
                var percentValue = servo.PercentCalculator.CalculatePercent(servo.TargetValue);
                servoPoint = new ServoPoint(servo.Id, percentValue, _playPosSynchronizer.PlayPosMsGuaranteedSnapped);
                _ = _timelineData?.AddPoint(servoPoint);
            }
            else
            {
                // Remove the existing servo point
                _ = _timelineData?.RemovePoint(servoPoint);
            }
            _timelineData!.SetContentChanged(TimelineDataChangedEventArgs.ChangeTypes.ServoPointChanged, servoPoint.ServoId);
            // todo: undo logic
        }

        #endregion

        #region SOUNDPLAYER

        /// <summary>
        /// Sets a sound player value at the current timeline position 
        /// </summary>
        public void UpdateSoundPlayerValue(ISoundPlayer soundPlayer, int? soundId, string? soundTitle)
        {
            var soundPoint = _timelineData?.SoundPoints.SingleOrDefault(p => p.SoundPlayerId == soundPlayer.Id && (int)p.TimeMs == _playPosSynchronizer.PlayPosMsGuaranteedSnapped); // check existing point
            if (soundId == null)
            {
                // sound should be removed from timeline at this position
                if (soundPoint == null) return; // nothing to do
                _ = _timelineData?.RemovePoint(_playPosSynchronizer.PlayPosMsGuaranteedSnapped, soundPlayer.Id);
            }
            else
            {
                // sound should be added or updated at this position
                if (soundPoint == null)
                {
                    soundPoint = new SoundPoint(_playPosSynchronizer.PlayPosMsGuaranteedSnapped, soundPlayer.Id, soundTitle ?? $"Sound {soundId}", soundId.Value);
                    _ = _timelineData?.AddPoint(soundPoint);
                }
                else
                {
                    soundPoint.SoundId = soundId.Value;
                }
            }
            _timelineData!.SetContentChanged(TimelineDataChangedEventArgs.ChangeTypes.SoundPointChanged, soundPlayer.Id);
            // todo: undo logic
        }

        #endregion


        /// <summary>
        /// set a nested timeline value at the current timeline position
        /// </summary>
        public void UpdateNestedTimelinesValue()
        {
            var timelineId = NestedTimelinesFakeObject.Singleton.ActualNestedTimelineId;
            var nestedTimelinePoint = _timelineData?.NestedTimelinePoints.SingleOrDefault(p => (int)p.TimeMs == _playPosSynchronizer.PlayPosMsGuaranteedSnapped); // check existing point
            if (timelineId == null)
            {
                // nested timeline value should be removed from timeline at this position
                if (nestedTimelinePoint == null) return; // nothing to do
                _ = _timelineData?.RemovePoint(nestedTimelinePoint);
            }
            else
            {
                // nested timeline value should be added or updated at this position
                if (nestedTimelinePoint == null)
                {
                    nestedTimelinePoint = new NestedTimelinePoint(_playPosSynchronizer.PlayPosMsGuaranteedSnapped, timelineId);
                    _timelineData?.AddPoint(nestedTimelinePoint);
                }
                else
                {
                    nestedTimelinePoint.TimelineId = timelineId;
                }
            }
            _timelineData!.SetContentChanged(TimelineDataChangedEventArgs.ChangeTypes.NestedTimelinePointChanged, NestedTimelinesFakeObject.Singleton.Id);
            // todo: undo logic
        }
    }
}
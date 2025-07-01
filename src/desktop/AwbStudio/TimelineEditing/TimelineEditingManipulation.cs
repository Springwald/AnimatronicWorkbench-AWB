// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Actuators;
using Awb.Core.Player;
using Awb.Core.Project.Various;
using Awb.Core.Timelines;
using Awb.Core.Timelines.NestedTimelines;
using Awb.Core.Timelines.Sounds;
using System.Collections.Generic;
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
        private readonly CopyNPasteBufferHolder _copyNPasteBufferHolder;

        public CopyNPasteBuffer? CopyNPasteBuffer
        {
            get => _copyNPasteBufferHolder.CopyNPasteBuffer;
            set => _copyNPasteBufferHolder.CopyNPasteBuffer = value;
        }

        public TimelineEditingManipulation(TimelineData timelineData, CopyNPasteBufferHolder copyNPasteBufferHolder, PlayPosSynchronizer playPosSynchronizer)
        {
            _timelineData = timelineData;
            _playPosSynchronizer = playPosSynchronizer;
            _copyNPasteBufferHolder = copyNPasteBufferHolder;
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
            var originalPoints = _timelineData.AllPoints.Where(p => p.TimeMs >= startMs && p.TimeMs <= endMs);
            return new CopyNPasteBuffer { TimelinePoints = ReAlignPoints(originalPoints, -startMs).ToArray(), LengthMs = endMs - startMs };
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

            // copy the points to clipboard
            var copyNPasteBuffer = Copy(startMs, endMs);
            if (copyNPasteBuffer == null) return null; // todo: better error handling

            // remove the points between start and end
            if (Clear(startMs, endMs) == false) return null; // todo: better error handling

            // close the gap between start and end
            if (MovePoints(oldStartMs: endMs, oldEndMs: int.MaxValue, newStartMs: startMs) == false) return null; // todo: better error handling

            _timelineData.SetContentChanged(TimelineDataChangedEventArgs.ChangeTypes.CopyNPaste, changedObjectId: null);

            return copyNPasteBuffer;
            // todo: undo logic
        }

        /// <summary>
        /// Inserts the timeline content from the copy buffer into the timeline at the given position.
        /// The timeline gets longer by the duration of the copied content.
        /// </summary>
        public bool Paste(CopyNPasteBuffer copyNPasteBuffer, int targetMs)
        {
            if (copyNPasteBuffer == null) return false;

            var originalPoints = copyNPasteBuffer.TimelinePoints;
            var reAlignedPoints = ReAlignPoints(originalPoints, targetMs).ToArray(); // re-align the points to insert

            // create a gap for the clipboard content
            if (MovePoints(oldStartMs: targetMs, oldEndMs: int.MaxValue, newStartMs: targetMs + copyNPasteBuffer.LengthMs) == false)
                return false; // todo: better error handling

            // insert he clipboard content
            foreach (var point in reAlignedPoints)
            {
                var collidingPointAtTargetPosition = _timelineData.AllPoints.Where(p => p.TimeMs == point.TimeMs && p.AbwObjectId == point.AbwObjectId).SingleOrDefault();
                if (collidingPointAtTargetPosition != null)
                {
                    // there is a point for the same awb-object at the target position, so we have to remove it first
                    _ = _timelineData.RemovePoint(collidingPointAtTargetPosition);
                }
                // insert the new point
                _ = _timelineData.AddPoint(point);
            }
            _timelineData.SetContentChanged(TimelineDataChangedEventArgs.ChangeTypes.CopyNPaste, changedObjectId: null);
            return true;
            // todo: undo logic
        }

        /// <summary>
        /// Clears the timeline content between the given start and end position.
        /// Does not remove the space between the points, only the points themselves.
        /// </summary>
        public bool Clear(int startMs, int endMs)
        {
            _timelineData.SetContentChanged(TimelineDataChangedEventArgs.ChangeTypes.CopyNPaste, changedObjectId: null);
            var pointsToRemove = _timelineData.AllPoints.Where(p => p.TimeMs >= startMs && p.TimeMs <= endMs).ToList();
            foreach (var point in pointsToRemove)
                _ = _timelineData.RemovePoint(point);
            return true;
            // todo: undo logic
        }

        /// <summary>
        /// sets the timeline content between the given start and end position to the timebase
        /// </summary>
        private IEnumerable<TimelinePoint> ReAlignPoints(IEnumerable<TimelinePoint> points, int deltaMs)
        {
            foreach (var originalPoint in points)
            {
                var point = originalPoint.Clone();
                point.TimeMs = point.TimeMs + deltaMs;
                yield return point;
            }
        }

        /// <summary>
        /// moves the timeline points between the given start and end position to the new start position
        /// </summary>
        private bool MovePoints(int oldStartMs, int oldEndMs, int newStartMs)
        {
            var pointsBetween = _timelineData.AllPoints.Where(p => p.TimeMs >= oldStartMs && p.TimeMs <= oldEndMs).ToList();
            var delta = newStartMs - oldStartMs;
            foreach (var point in pointsBetween)
                point.TimeMs = point.TimeMs + delta;
            _timelineData.SetContentChanged(TimelineDataChangedEventArgs.ChangeTypes.CopyNPaste, changedObjectId: null);
            return true;
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
        public void UpdateSoundPlayerValue(ISoundPlayer soundPlayer, int? soundId, string? soundTitle, ActuatorMovementBySound[] actuatorMovementBySound)
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
                    soundPoint = new SoundPoint(_playPosSynchronizer.PlayPosMsGuaranteedSnapped, soundPlayer.Id, soundTitle ?? $"Sound {soundId}", soundId.Value, actuatorMovementBySound);
                    _ = _timelineData?.AddPoint(soundPoint);
                }
                else
                {
                    soundPoint.SoundId = soundId.Value;
                    soundPoint.ActuatorMovementsBySound = actuatorMovementBySound;
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
// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Project;
using Awb.Core.Timelines.NestedTimelines;
using System.Drawing;

namespace Awb.Core.Timelines
{
    public class TimelineData
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public int TimelineStateId { get; set; }
        public int? NextTimelineStateIdOnce { get; set; }

        public string Title { get; set; } = "no name";

        private List<TimelinePoint> _timelinePoints = new List<TimelinePoint>();

        /// <summary>
        /// The content of the timeline has changed
        /// </summary>
        public EventHandler<TimelineDataChangedEventArgs>? OnContentChanged;

        /// <summary>
        /// What is the duration of the timeline filled with points?
        /// </summary>
        public int DurationMs => AllPoints?.Any() == true ? AllPoints.Max(p => p.TimeMs) : 0;

        public IEnumerable<TimelinePoint> AllPoints => _timelinePoints.ToArray();

        /// <summary>
        /// All servo values changes of the timeline are stored as single points.
        /// Returns all servo points of _timelinePoints
        /// </summary>
        public IEnumerable<ServoPoint> ServoPoints => _timelinePoints.OfType<ServoPoint>();

        /// <summary>
        /// All sound events of the timeline are stored as single points
        /// </summary>
        public IEnumerable<SoundPoint> SoundPoints => _timelinePoints.OfType<SoundPoint>();

        /// <summary>
        /// All nested timelines are stored as single points
        /// </summary>
        public IEnumerable<NestedTimelinePoint> NestedTimelinePoints => _timelinePoints.OfType<NestedTimelinePoint>();

        public int LatestPointMs => AllPoints.Max(p => p.TimeMs);

        public TimelineData(string id, List<ServoPoint> servoPoints, List<SoundPoint> soundPoints, List<NestedTimelinePoint> nestedTimelinePoints, int timelineStateId, int? nextTimelineStateIdOnce)
        {
            Id = id;
            TimelineStateId = timelineStateId;
            NextTimelineStateIdOnce = nextTimelineStateIdOnce;
            _timelinePoints.AddRange(servoPoints);
            _timelinePoints.AddRange(soundPoints);
            _timelinePoints.AddRange(nestedTimelinePoints);
        }

        /// <summary>
        /// inserts a point into the timeline
        /// </summary>
        public TimelinePoint AddPoint(TimelinePoint point)
        {
            if (point == null) throw new ArgumentNullException(nameof(point));
            var collidingPoint = _timelinePoints.GetPoint(LatestPointMs, point.AbwObjectId);
            if (collidingPoint != null) throw new ArgumentOutOfRangeException($"Point '{point.Description}' collides with existing point '{collidingPoint.Description}'.");
            _timelinePoints.Add(point);
            SetContentChangedByPoint(point);
            return point;
        }

        /// <summary>
        /// removes a point from the timeline
        /// </summary>
        public bool RemovePoint(int timeMs, string awbObjectId)         {
            var point = AllPoints.GetPoint(timeMs, awbObjectId);
            return RemovePoint(point);
        }

        /// <summary>
        /// removes a point from the timeline
        /// </summary>
        public bool RemovePoint(TimelinePoint? timelinePoint)
        {
            if (timelinePoint == null) return false;
            if (!_timelinePoints.Remove(timelinePoint)) throw new ArgumentOutOfRangeException($"Point '{timelinePoint.Description}' could not be removed from timeline.");
            SetContentChangedByPoint(timelinePoint);
            return true;
        }

        /// <summary>
        /// Announce that the content of the timeline has changed
        /// </summary>
        public void SetContentChanged(TimelineDataChangedEventArgs.ChangeTypes changeType, string? changedObjectId)
            => OnContentChanged?.Invoke(this, new TimelineDataChangedEventArgs(changeType, changedObjectId));


        private void SetContentChangedByPoint(TimelinePoint point)
        {
            if (point is ServoPoint servoPoint)
            {
                SetContentChanged(TimelineDataChangedEventArgs.ChangeTypes.ServoPointChanged, point.AbwObjectId);
            }
            else if (point is SoundPoint soundPoint)
            {
                SetContentChanged(TimelineDataChangedEventArgs.ChangeTypes.SoundPointChanged, point.AbwObjectId);
            }
            else if (point is NestedTimelinePoint nestedTimelinePoint)
            {
                SetContentChanged(TimelineDataChangedEventArgs.ChangeTypes.NestedTimelinePointChanged, point.AbwObjectId);
            }
            else
            {
                throw new ArgumentOutOfRangeException($"Point type {typeof(Point)} not supported for content change event.");
            }
        }

        /// <summary>
        /// Get problems of this timeline, e.g. no existing servo or sound objects referenced
        /// </summary>
        /// <param name="awbProject">the project data as a reference</param>
        /// <returns></returns>
        public IEnumerable<ProjectProblem> GetProblems(AwbProject awbProject)
        {
            yield break;
            //throw new NotImplementedException();
        }
    }
}

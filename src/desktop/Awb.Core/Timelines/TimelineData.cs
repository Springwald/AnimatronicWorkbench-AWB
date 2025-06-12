// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Project;
using Awb.Core.Services;
using Awb.Core.Sounds;
using Awb.Core.Timelines.NestedTimelines;
using Awb.Core.Timelines.Sounds;
using System.Drawing;
using System.Reflection.Metadata.Ecma335;

namespace Awb.Core.Timelines
{
    public class TimelineData
    {
        private int? _durationMsCache;

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
        public int GetDurationMs(Sound[] projectSounds, ITimelineDataService timelineDataService, int recursionDepth=0)
        {
            if (_durationMsCache .HasValue) return _durationMsCache.Value;

            recursionDepth++;

            var duration = 0;

            foreach(var point in _timelinePoints)
            {
                if (point.TimeMs > duration)
                {
                    if (point is NestedTimelinePoint nestedPoint)
                    {
                        // if this is a nested timeline, we need to get the duration of the nested timeline
                        var nestedTimeline = timelineDataService.GetTimelineData(nestedPoint.TimelineId);
                        if (nestedTimeline != null)
                        {
                            if (recursionDepth > 10)
                            {
                                // prevent infinite recursion
                               duration = point.TimeMs + 1000; // add a default duration of 1 second
                            }
                            else
                            {
                                var nestedDuration = nestedTimeline.GetDurationMs(projectSounds, timelineDataService, recursionDepth);
                                duration = Math.Max(duration, point.TimeMs + nestedDuration);
                            }
                        }
                        else
                        {
                            // if the nested timeline does not exist, we cannot calculate the duration
                            duration = Math.Max(duration, point.TimeMs);
                        }
                    }
                    else if (point is SoundPoint soundPoint)
                    {
                        // if this is a sound point, we need to get the duration of the sound
                        var sound = projectSounds.FirstOrDefault(s => s.Id == soundPoint.SoundId);  
                        var soundDuration = sound?.DurationMs ?? 0;
                        duration = Math.Max(duration, point.TimeMs + soundDuration);
                    } else 
                        duration = Math.Max(duration, point.TimeMs);
                }
            }

            _durationMsCache = duration;

            return duration;
        } 

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
            var collidingPoint = _timelinePoints.GetPoint(point.TimeMs, point.AbwObjectId);
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
        {
            OnContentChanged?.Invoke(this, new TimelineDataChangedEventArgs(changeType, changedObjectId));
            _durationMsCache = null;
        }


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

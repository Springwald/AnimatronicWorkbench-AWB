// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using System.Drawing;
using Awb.Core.Project;
using Awb.Core.Timelines.NestedTimelines;

namespace Awb.Core.Timelines
{
    public class TimelineData
    {
        public string Id { get; set; } = Guid.NewGuid().ToString(); 

        public int TimelineStateId { get; set; }

        public string Title { get; set; } = "no name";

        /// <summary>
        /// The content of the timeline has changed
        /// </summary>
        public EventHandler<TimelineDataChangedEventArgs>? OnContentChanged;

        /// <summary>
        /// What is the duration of the timeline filled with points?
        /// </summary>
        public int DurationMs => AllPoints?.Any() == true ? AllPoints.Max(p => p.TimeMs) : 0;

        public IEnumerable<TimelinePoint> AllPoints
        {
            get
            {
                foreach (var point in ServoPoints) yield return point;
                foreach (var point in SoundPoints) yield return point;
                foreach (var point in NestedTimelinePoints) yield return point;
            }
        }

        /// <summary>
        /// All servo values changes of the timeline are stored as single points
        /// </summary>
        public List<ServoPoint> ServoPoints { get;  }

        /// <summary>
        /// All sound events of the timeline are stored as single points
        /// </summary>
        public List<SoundPoint> SoundPoints { get;  }

        /// <summary>
        /// All nested timelines are stored as single points
        /// </summary>
        public List<NestedTimelinePoint> NestedTimelinePoints { get;  }

        public int LatestPointMs => AllPoints.Max(p => p.TimeMs);

        public TimelineData(string id, List<ServoPoint> servoPoints, List<SoundPoint> soundPoints, List<NestedTimelinePoint> nestedTimelinePoints, int timelineStateId)
        {
            Id = id;
            TimelineStateId = timelineStateId;
            ServoPoints = servoPoints;
            SoundPoints = soundPoints;
            NestedTimelinePoints = nestedTimelinePoints;
        }

       

        public TimelinePoint InsertPoint(TimelinePoint point)
        {
            if (point == null) throw new ArgumentNullException(nameof(point));

            if (point is ServoPoint servoPoint)
            {
                ServoPoints.Add(servoPoint);
                SetContentChanged(TimelineDataChangedEventArgs.ChangeTypes.ServoPointChanged, point.AbwObjectId);
            }
            else if (point is SoundPoint soundPoint)
            {
                SoundPoints.Add(soundPoint);
                SetContentChanged(TimelineDataChangedEventArgs.ChangeTypes.SoundPointChanged, point.AbwObjectId);
            }
            else if (point is NestedTimelinePoint nestedTimelinePoint)
            {
                NestedTimelinePoints.Add(nestedTimelinePoint);
                SetContentChanged(TimelineDataChangedEventArgs.ChangeTypes.NestedTimelinePointChanged, point.AbwObjectId);
            }

            throw new ArgumentOutOfRangeException($"Point type {typeof(Point)} not supported for RemovePoint method.");
        }

        public bool RemovePoint<TimelinePointType>(int timeMs, string awbObjectId) where TimelinePointType : TimelinePoint
        {
            var point = AllPoints.GetPoint<TimelinePointType>(timeMs, awbObjectId);
            if (point == null) return false;

            if (point is ServoPoint servoPoint)
            {
                ServoPoints.Remove(servoPoint);
                SetContentChanged(TimelineDataChangedEventArgs.ChangeTypes.ServoPointChanged, point.AbwObjectId);
            } 
            else if (point is SoundPoint soundPoint)
            {
                SoundPoints.Remove(soundPoint);
                SetContentChanged(TimelineDataChangedEventArgs.ChangeTypes.SoundPointChanged, point.AbwObjectId);
            }
            else if (point is NestedTimelinePoint nestedTimelinePoint)
            {
                NestedTimelinePoints.Remove(nestedTimelinePoint);
                SetContentChanged(TimelineDataChangedEventArgs.ChangeTypes.NestedTimelinePointChanged, point.AbwObjectId);
            }

            throw new ArgumentOutOfRangeException($"Point type {typeof(Point)} not supported for RemovePoint method.");   
        }

        public void SetContentChanged(TimelineDataChangedEventArgs.ChangeTypes changeType, string? changedObjectId)
        {
            OnContentChanged?.Invoke(this, new TimelineDataChangedEventArgs(changeType, changedObjectId));
        }

        public void SetContentChangedByPoint(TimelinePoint point)
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

            throw new ArgumentOutOfRangeException($"Point type {typeof(Point)} not supported for content change event.");
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

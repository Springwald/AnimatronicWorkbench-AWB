// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

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
        public List<ServoPoint> ServoPoints { get; set; }

        /// <summary>
        /// All sound events of the timeline are stored as single points
        /// </summary>
        public List<SoundPoint> SoundPoints { get; set; }

        /// <summary>
        /// All nested timelines are stored as single points
        /// </summary>
        public List<NestedTimelinePoint> NestedTimelinePoints { get; set; }

        public int LatestPointMs => AllPoints.Max(p => p.TimeMs);

        public TimelineData(string id, List<ServoPoint> servoPoints, List<SoundPoint> soundPoints, List<NestedTimelinePoint> nestedTimelinePoints, int timelineStateId)
        {
            Id = id;
            TimelineStateId = timelineStateId;
            ServoPoints = servoPoints;
            SoundPoints = soundPoints;
            NestedTimelinePoints = nestedTimelinePoints;
        }
        public void SetContentChanged(TimelineDataChangedEventArgs.ChangeTypes changeType, string? changedObjectId)
        {
            OnContentChanged?.Invoke(this, new TimelineDataChangedEventArgs(changeType, changedObjectId));
        }
    }
}

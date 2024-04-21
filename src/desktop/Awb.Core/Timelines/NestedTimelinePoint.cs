// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Project;

namespace Awb.Core.Timelines
{
    public class NestedTimelinePoint : TimelinePoint
    {
        /// <summary>
        /// The id of the timeline to be inserted into the current timeline.
        /// </summary>
        public string TimelineId { get; set; }

        public override string Title { get; set; }

        public override string PainterCheckSum => TimelineId.ToString() + base.TimeMs.ToString() ;

        /// <param name="soundId">The resource id of the sound to be played. What kind of resource this is depends on the implementation of the sound player.</param>
        public NestedTimelinePoint(int timeMs, string timelineId) : base(targetObjectId: NestedTimelinesFakeObject.Singleton.Id, timeMs: timeMs)
        {
            this.TimeMs = timeMs;
            this.TimelineId = timelineId;
            this.Title = "Nested Timelines";
        }
    }
}
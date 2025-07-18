﻿// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License


using Awb.Core.Project.Various;

namespace Awb.Core.Timelines.NestedTimelines
{
    public class NestedTimelinePoint : TimelinePoint
    {
        /// <summary>
        /// The id of the timeline to be inserted into the current timeline.
        /// </summary>
        public string TimelineId { get; set; }

        public override string Title => "Nested Timelines";

        public override string PainterCheckSum => TimelineId.ToString() + TimeMs.ToString();

        /// <param name="soundId">The resource id of the sound to be played. What kind of resource this is depends on the implementation of the sound player.</param>
        public NestedTimelinePoint(int timeMs, string timelineId) : base(targetObjectId: NestedTimelinesFakeObject.Singleton.Id, timeMs: timeMs)
        {
            TimeMs = timeMs;
            TimelineId = timelineId;
        }

        public override NestedTimelinePoint Clone()
        {
            return new NestedTimelinePoint(timeMs: TimeMs, timelineId: TimelineId);
        }
    }
}
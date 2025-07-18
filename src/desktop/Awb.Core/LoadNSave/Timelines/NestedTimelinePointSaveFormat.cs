﻿// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Timelines.NestedTimelines;

namespace Awb.Core.LoadNSave.Timelines
{
    public class NestedTimelinePointSaveFormat
    {
        public int TimeMs { get; set; }
        public string TimelineId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }

        public NestedTimelinePointSaveFormat(int timeMs, string timelineId, string title)
        {
            TimeMs = timeMs;
            TimelineId = timelineId;
            Title = title;
        }

        public static NestedTimelinePointSaveFormat FromNestedTimelinePoint(NestedTimelinePoint nestedTimelinePoint) =>
            new NestedTimelinePointSaveFormat(
                timeMs: nestedTimelinePoint.TimeMs,
                timelineId: nestedTimelinePoint.TimelineId,
                title: nestedTimelinePoint.Title)
            {
                Title = nestedTimelinePoint.Title,
                Description = nestedTimelinePoint.Description,
            };

        public static NestedTimelinePoint ToNestedTimelinePoint(NestedTimelinePointSaveFormat nestedTimelinePointSaveFormat)
        => new NestedTimelinePoint(
            timeMs: nestedTimelinePointSaveFormat.TimeMs,
            timelineId: nestedTimelinePointSaveFormat.TimelineId
            )
        {
            Description = nestedTimelinePointSaveFormat.Description,
        };
    }
}

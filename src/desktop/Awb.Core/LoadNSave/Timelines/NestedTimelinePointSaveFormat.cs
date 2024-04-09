// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Timelines;

namespace Awb.Core.LoadNSave.Timelines
{
    public class NestedTimelinePointSaveFormat
    {
        public int TimeMs { get; set; }
        public string TimelineId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }

        public NestedTimelinePointSaveFormat(int timeMs, string timelineId)
        {
            TimeMs = timeMs;
            TimelineId = timelineId;
        }

        public static NestedTimelinePointSaveFormat FromNestedTimelinePoint(NestedTimelinePoint nestedTimelinePoint) => new NestedTimelinePointSaveFormat(
                timeMs: nestedTimelinePoint.TimeMs,
                timelineId: nestedTimelinePoint.TimelineId
                )
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
                Title = nestedTimelinePointSaveFormat.Title,
                Description = nestedTimelinePointSaveFormat.Description,
            };
    }
}

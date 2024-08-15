// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Services;
using Awb.Core.Timelines;
using Awb.Core.Timelines.NestedTimelines;

namespace Awb.Core.Export.ExporterParts.ExportData
{
    public class TimelineExportData
    {
        public int TimelineStateId { get; set; }
        public int? NextTimelineStateOnceId { get; set; }
        public string Title { get; set; }
        public TimelinePoint[] Points { get; set; }

        private TimelineExportData(string title, int timelineStateId, int? nextTimelineStateIdOnce, TimelinePoint[] points)
        {
            Title = title;
            TimelineStateId = timelineStateId;
            NextTimelineStateOnceId = nextTimelineStateIdOnce;
            Points = points;
        }

        public static TimelineExportData FromTimeline(int timelineStateId, int? nextTimelineStateIdOnce, string title, IEnumerable<TimelinePoint> points, ITimelineDataService timelineDataService, IAwbLogger awbLogger)
        {
            var merger = new NestedTimelinesPointMerger(points, timelineDataService, awbLogger, recursionDepth: 0);
            return new TimelineExportData(title: title, timelineStateId: timelineStateId, nextTimelineStateIdOnce: nextTimelineStateIdOnce, points: merger.MergedPoints.ToArray());
        }
    }
}

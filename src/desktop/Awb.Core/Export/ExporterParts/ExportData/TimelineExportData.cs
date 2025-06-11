// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Services;
using Awb.Core.Sounds;
using Awb.Core.Timelines;

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

        public static TimelineExportData FromTimeline(int timelineStateId, int? nextTimelineStateIdOnce, string title, IEnumerable<TimelinePoint> points, Sound[] projectSounds, ITimelineDataService timelineDataService, IAwbLogger awbLogger)
        {
            var merger = new EverythingMerger(timelineDataService, projectSounds, awbLogger);
            var mergedPoints = merger.Merge(points);
            return new TimelineExportData(title: title, timelineStateId: timelineStateId, nextTimelineStateIdOnce: nextTimelineStateIdOnce, points: mergedPoints.ToArray());
        }
    }
}

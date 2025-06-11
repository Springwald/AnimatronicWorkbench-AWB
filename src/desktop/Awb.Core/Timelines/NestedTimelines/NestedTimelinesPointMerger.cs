// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Services;

namespace Awb.Core.Timelines.NestedTimelines
{
    public class NestedTimelinesPointMerger
    {
        private const int MaxRecursionDepth = 10;

        private readonly IEnumerable<TimelinePoint> _rawPoints;
        private readonly ITimelineDataService _timelineDataService;
        private readonly IAwbLogger _awbLogger;
        private readonly int _recursionDepth;

        public IEnumerable<TimelinePoint> MergedPoints
        {
            get
            {
                foreach (var point in _rawPoints)
                {
                    if (point is NestedTimelinePoint nestedTimelinePoint)
                    {
                        if (_recursionDepth < MaxRecursionDepth)
                        {
                            var nestedTimelinePoints = _timelineDataService.GetTimelineData(nestedTimelinePoint.TimelineId).AllPoints;
                            var merger = new NestedTimelinesPointMerger(nestedTimelinePoints, _timelineDataService, _awbLogger, _recursionDepth + 1);
                            foreach (var nestedPoint in merger.MergedPoints)
                            {
                                var clone = nestedPoint.Clone();
                                clone.TimeMs += nestedTimelinePoint.TimeMs;
                                clone.IsNestedTimelinePoint = true;
                                yield return clone;
                            }
                        } else
                        {
                            _awbLogger.LogErrorAsync($"Nested timeline recursion depth exceeded " + MaxRecursionDepth + $" for timeline {nestedTimelinePoint.TimelineId}!");
                        }
                    }
                    else
                        yield return point;
                }
            }
        }

        public NestedTimelinesPointMerger(IEnumerable<TimelinePoint> rawPoints, ITimelineDataService timelineDataService, IAwbLogger awbLogger, int recursionDepth = 0)
        {
            _rawPoints = rawPoints;
            _timelineDataService = timelineDataService;
            _awbLogger = awbLogger;
            _recursionDepth = recursionDepth;
        }

    }
}

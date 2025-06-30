// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Services;

namespace Awb.Core.Timelines.NestedTimelines
{
    public class NestedTimelinesPointMerger : IMerger
    {
        private const int MaxRecursionDepth = 10;

        private readonly ITimelineDataService _timelineDataService;
        private readonly IAwbLogger _awbLogger;

        public IEnumerable<TimelinePoint> Merge(IEnumerable<TimelinePoint> rawPoints, int recursionDepth = 0)
        {
            foreach (var point in rawPoints)
            {
                if (point is NestedTimelinePoint nestedTimelinePoint)
                {
                    if (recursionDepth < MaxRecursionDepth)
                    {
                        var timelineData = _timelineDataService.GetTimelineData(nestedTimelinePoint.TimelineId);
                        if (timelineData == null)
                        {
                            _awbLogger.LogErrorAsync($"Nested timeline with ID {nestedTimelinePoint.TimelineId} not found!");
                            continue;
                        }
                        var nestedTimelinePoints = timelineData.AllPoints;
                        var pointsFromNestedTimeline = Merge(nestedTimelinePoints, recursionDepth + 1);
                        foreach (var nestedPoint in pointsFromNestedTimeline)
                        {
                            var clone = nestedPoint.Clone();
                            clone.TimeMs += nestedTimelinePoint.TimeMs;
                            clone.IsNestedTimelinePoint = true;
                            yield return clone;
                        }
                    }
                    else
                    {
                        _awbLogger.LogErrorAsync($"Nested timeline recursion depth exceeded " + MaxRecursionDepth + $" for timeline {nestedTimelinePoint.TimelineId}!");
                    }
                }
                else
                    yield return point;
            }
        }

        public NestedTimelinesPointMerger(ITimelineDataService timelineDataService, IAwbLogger awbLogger)
        {
            _timelineDataService = timelineDataService;
            _awbLogger = awbLogger;
        }
    }
}

// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

namespace Awb.Core.Timelines
{
    public static class TimelinePointFinderExtensions
    {
        public static TimelinePointType? GetPoint<TimelinePointType>(this IEnumerable<TimelinePoint> points, int timeMs, string awbObjectId) where TimelinePointType : TimelinePoint
           => points.OfType<TimelinePointType>().SingleOrDefault(p => p.AbwObjectId == awbObjectId && (int)p.TimeMs == timeMs); // check existing point

        public static IEnumerable<TimelinePointType> GetPointsBetween<TimelinePointType>(this IEnumerable<TimelinePoint> points, int timeMs1, int timeMs2, string awbObjectId) where TimelinePointType : TimelinePoint
        {
            var lower = Math.Min(timeMs1, timeMs2);
            var higher = Math.Max(timeMs1, timeMs2);
            var pointsWithMatchingMs = points.OfType<TimelinePointType>().Where(p => p.AbwObjectId == awbObjectId && p.TimeMs >= lower && p.TimeMs <= higher);
            foreach (var point in pointsWithMatchingMs)
                yield return point;
        }
    }
}

//// Animatronic WorkBench core routines
//// https://github.com/Springwald/AnimatronicWorkBench-AWB
////
//// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
//// https://daniel.springwald.de - daniel@springwald.de
//// All rights reserved   -  Licensed under MIT License

//using Awb.Core.Services;

//namespace Awb.Core.Timelines.NestedTimelines
//{
//    public class NestedTimeline
//    {
//        public string Id { get; set; }
//        public int NestedStartMs { get; set; }
//        public int NestedEndMs { get; set; }

//        public IEnumerable<TimelinePoint> Points { get; }

//        public NestedTimeline(string id, int nestedStartMs, ITimelineDataService timelineDataService)
//        {
//            Id = id;
//            NestedStartMs = nestedStartMs;
            
//            var data = timelineDataService.GetTimelineData(id);
//            if (data == null)
//                throw new Exception($"Timeline with id {id} not found");

//            if (data.AllPoints.Any(p => p is NestedTimelinePoint))
//                throw new Exception($"Nested timeline with id {id} contains nested timeline points!");

//            Points = data.AllPoints.Select(p => GetPoint(p, nestedStartMs)).OrderBy(p => p.TimeMs).ToArray();
//            NestedEndMs = Points.LastOrDefault()?.TimeMs ?? nestedStartMs;
//        }

//        private TimelinePoint GetPoint(TimelinePoint p, int startMs)
//        {
//            var clone = p.Clone();
//            clone.TimeMs+= startMs;
//            return clone;
//        }
//    }
//}

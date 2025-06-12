// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Actuators;
using Awb.Core.Services;
using Awb.Core.Sounds;
using Awb.Core.Timelines.NestedTimelines;
using Awb.Core.Timelines.Sounds;

namespace Awb.Core.Timelines
{
    public interface IMerger
    {
        IEnumerable<TimelinePoint> Merge(IEnumerable<TimelinePoint> rawPoint, int recursionDepth = 0);
    }

    public class EverythingMerger : IMerger
    {
        private readonly ITimelineDataService _timelineDataService;
        private readonly IAwbLogger _awbLogger;
        private readonly List<IMerger> _mergers;

        public IEnumerable<TimelinePoint> Merge(IEnumerable<TimelinePoint> points, int recursionDepth = 0)
        {
            foreach (var merger in _mergers)
                points = merger.Merge(points, recursionDepth).ToList();

            return points;
        }

        public EverythingMerger( ITimelineDataService timelineDataService, Sound[] projectSounds, IServo[] projectServos, IAwbLogger awbLogger)
        {
            _timelineDataService = timelineDataService;
            _awbLogger = awbLogger;
            _mergers = new List<IMerger>
            {
                new NestedTimelinesPointMerger( _timelineDataService, _awbLogger),
                new SoundsPointMerger(projectSounds, projectServos, _awbLogger)
            };
        }
    }
}

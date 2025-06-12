// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Timelines;

namespace Awb.Core.Services
{
    public interface ITimelineDataService
    {
        ITimelineMetaDataService TimelineMetaDataService { get; }

        IEnumerable<string> TimelineIds { get; }

        bool Exists(string timelineId);

        TimelineData? GetTimelineData(string timelineId);
        bool SaveTimelineData(TimelineData data);
    }
}

// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Timelines;

namespace Awb.Core.Services
{
    public interface ITimelineDataService
    {
        ITimelineMetaDataService TimelineMetaDataService { get; }

        IEnumerable<string> TimelineIds { get; }

        bool Exists(string timelineId);

        TimelineData? GetTimelineData(string timelineId);

        IEnumerable<TimelineData> GetAllTimelinesDatas();

        bool SaveTimelineData(TimelineData data);
    }
}

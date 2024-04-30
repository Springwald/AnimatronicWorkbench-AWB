// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

namespace Awb.Core.Timelines
{
    public class TimelineMetaData
    {
        public string Id { get; set; }
        public string Title { get; }
        public int StateId { get; }
        public int DurationMs { get;  }

        public TimelineMetaData(string id, string title, int stateId, int durationMs)
        {
            Id = id;
            Title = title;
            StateId = stateId;
            DurationMs = durationMs;
        }
    }
}

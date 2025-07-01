// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

namespace Awb.Core.Timelines
{
    public class TimelineMetaData(string id, string title, int stateId, int durationMs)
    {
        public string Id { get; set; } = id;
        public string Title { get; } = title;
        public int StateId { get; } = stateId;
        public int DurationMs { get; } = durationMs;
    }
}

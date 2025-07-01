// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

namespace AwbStudio.TimelineEditing
{
    public class TimelineNameChosenEventArgs(string timelineId)
    {
        public string TimelineId { get; } = timelineId;
    }
}

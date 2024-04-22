// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

namespace AwbStudio.TimelineEditing
{
    public class TimelineNameChosenEventArgs
    {
        public string TimelineId { get; }

        public TimelineNameChosenEventArgs(string timelineId)
        {
            TimelineId = timelineId;
        }
    }
}

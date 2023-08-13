// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

namespace AwbStudio.TimelineControls
{
    public class TimelineViewPos
    {
        /// <summary>
        /// How many seconds are displayed in the timeline
        /// </summary>
        public int DisplayMs { get; set; } = 10* 1000;

        /// <summary>
        /// The left offset of the timeline in seconds
        /// </summary>
        public int ScrollOffsetMs { get; set; } = 0;
    }
}

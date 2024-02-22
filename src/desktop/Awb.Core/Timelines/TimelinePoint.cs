// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

namespace Awb.Core.Timelines
{
    public abstract class TimelinePoint
    {
        /// <summary>
        /// The time in the timeline when this point should be reached, counted from the start of the timeline
        /// </summary>
        public int TimeMs { get; set; }

        /// <summary>
        /// The title of this point
        /// </summary>
        public abstract string? Title { get;  }

        /// <summary>
        /// A description of this point, e.g. for documentation 
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// The id of the servo or display or other actuators
        /// </summary>
        public string TargetObjectId { get; set; }

        public TimelinePoint(string targetObjectId, int timeMs)
        {
            TargetObjectId = targetObjectId;
            TimeMs = timeMs;
        }
    }
}

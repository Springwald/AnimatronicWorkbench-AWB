﻿// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
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

        public bool IsNestedTimelinePoint { get; set; }

        /// <summary>
        /// The title of this point
        /// </summary>
        public abstract string Title { get;  }

        /// <summary>
        /// A description of this point, e.g. for documentation 
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// A checksum of the content of this point, e.g. for detecting changes
        /// </summary>
        public abstract string PainterCheckSum { get; }

        public abstract TimelinePoint Clone();


        /// <summary>
        /// The id of the servo or display or other actuators
        /// </summary>
        public string AbwObjectId { get; set; }

        public TimelinePoint(string targetObjectId, int timeMs)
        {
            AbwObjectId = targetObjectId;
            TimeMs = timeMs;
        }
    }
}

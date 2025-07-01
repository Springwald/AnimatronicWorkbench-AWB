// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

namespace Awb.Core.Timelines
{
    public abstract class TimelinePoint(string targetObjectId, int timeMs)
    {
        /// <summary>
        /// The time in the timeline when this point should be reached, counted from the start of the timeline
        /// </summary>
        public int TimeMs { get; set; } = timeMs;

        /// <summary>
        /// The id of the servo or display or other actuators
        /// </summary>
        public string AbwObjectId { get; set; } = targetObjectId;

        public bool IsNestedTimelinePoint { get; set; }

        /// <summary>
        /// The title of this point
        /// </summary>
        public abstract string Title { get; }

        /// <summary>
        /// A description of this point, e.g. for documentation 
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// A checksum of the content of this point, e.g. for detecting changes
        /// </summary>
        public abstract string PainterCheckSum { get; }

        public abstract TimelinePoint Clone();
    }
}

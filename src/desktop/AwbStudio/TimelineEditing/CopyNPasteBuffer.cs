// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Timelines;
using System.Collections.Generic;

namespace AwbStudio.TimelineEditing
{
    /// <summary>
    /// The buffer to hold timeline keyframes for copy and paste operations
    /// </summary>
    public sealed record CopyNPasteBuffer
    {
        /// <summary>
        /// the timeline content to copy or paste is defined by the timeline points
        /// </summary>
        public required IEnumerable<TimelinePoint> TimelinePoints { get; init; }

        /// <summary>
        /// where was the timeline content copied or cut from (start in milliseconds) 
        /// </summary>
        public required int OldStartMs { get; init; }

        /// <summary>
        /// where was the timeline content copied or cut from (end in milliseconds)
        /// </summary>
        public required int OldEndMs { get; init; }


    }
}

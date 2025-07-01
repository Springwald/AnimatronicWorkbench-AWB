// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Timelines;
using System.Collections.Generic;

namespace AwbStudio.TimelineEditing
{
    public sealed class CopyNPasteBufferHolder
    {
        public CopyNPasteBuffer? CopyNPasteBuffer { get; set; }
    }

    /// <summary>
    /// The buffer to hold timeline keyframes for copy and paste operations
    /// </summary>
    public sealed class CopyNPasteBuffer
    {
        /// <summary>
        /// the timeline content to copy or paste is defined by the timeline points
        /// </summary>
        public required IEnumerable<TimelinePoint> TimelinePoints { get; init; }

        public required int LengthMs { get; init; }

    }
}

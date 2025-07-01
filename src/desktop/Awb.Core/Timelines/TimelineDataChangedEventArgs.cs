// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

namespace Awb.Core.Timelines
{
    public class TimelineDataChangedEventArgs(TimelineDataChangedEventArgs.ChangeTypes changeType, string? changedObjectId) : EventArgs
    {
        public ChangeTypes ChangeType { get; } = changeType;

        public string? ChangedObjectId { get; } = changedObjectId;

        public enum ChangeTypes
        {
            ServoPointChanged,
            SoundPointChanged,
            NestedTimelinePointChanged,
            CopyNPaste
        }
    }
}

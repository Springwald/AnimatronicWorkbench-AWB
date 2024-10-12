// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

namespace Awb.Core.Timelines
{
    public class TimelineDataChangedEventArgs : EventArgs
    {
        public ChangeTypes ChangeType { get; }

        public string? ChangedObjectId { get; }

        public enum ChangeTypes
        {
            ServoPointChanged,
            SoundPointChanged,
            NestedTimelinePointChanged,
            CopyNPaste
        }

        public TimelineDataChangedEventArgs(ChangeTypes changeType, string? changedObjectId)
        {
            ChangeType = changeType;
            ChangedObjectId = changedObjectId;
        }
    }
}

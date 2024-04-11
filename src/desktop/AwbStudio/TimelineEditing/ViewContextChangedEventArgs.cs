// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using System;

namespace AwbStudio.TimelineEditing
{
    public class ViewContextChangedEventArgs : EventArgs
    {
        public ChangeTypes ChangeType { get; }

        public enum ChangeTypes
        {
            Duration,
            BankIndex,
            PixelPerMs,
            Scroll,
        }

        public ViewContextChangedEventArgs(ChangeTypes changeType)
        {
            ChangeType = changeType;
        }
    }
}

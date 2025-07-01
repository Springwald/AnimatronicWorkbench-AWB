// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using System;

namespace AwbStudio.TimelineEditing
{
    public class ViewContextChangedEventArgs(ViewContextChangedEventArgs.ChangeTypes changeType) : EventArgs
    {
        public ChangeTypes ChangeType { get; } = changeType;

        public enum ChangeTypes
        {
            Duration,
            BankIndex,
            PixelPerMs,
            Scroll,
            Selection,
            FocusObject,
            FocusObjectValue,
        }
    }
}

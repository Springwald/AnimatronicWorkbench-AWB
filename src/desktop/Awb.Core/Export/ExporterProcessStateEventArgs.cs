// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

namespace Awb.Core.Export
{
    public class ExporterProcessStateEventArgs
    {
        public enum ProcessStates
        {
            Error,
            Message,
            OnlyLog
        }

        public required ProcessStates State { get; init; }
        public required string Message { get; init; }
    }
}

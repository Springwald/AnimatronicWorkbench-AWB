// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

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

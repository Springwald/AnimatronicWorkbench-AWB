// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

namespace Awb.Core.InputControllers.Midi
{
    public class MidiInputEventArgs : EventArgs
    {
        public string InputId { get; set; }
        public string Value { get; set; }

        public MidiInputEventArgs(string inputId, string value)
        {
            this.InputId = inputId;
            this.Value = value;
        }
    }
}

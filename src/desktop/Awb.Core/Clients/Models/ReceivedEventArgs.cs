// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

namespace Awb.Core.Clients.Models
{
    public class ReceivedEventArgs : EventArgs
    {
        public string Payload { get; }

        public ReceivedEventArgs(string payload)
        {
            Payload = payload;
        }
    }
}

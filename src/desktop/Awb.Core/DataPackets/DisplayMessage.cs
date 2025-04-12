// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using System.Text.Json.Serialization;

namespace Awb.Core.DataPackets
{
    public class DisplayMessage
    {
        [JsonPropertyName("Msg")]
        public string Message { get; set; }

        [JsonPropertyName("Ms")]
        public int DurationMs { get; set; }

        public DisplayMessage(string message, int durationMs = 1000)
        {
            this.Message = message;
            this.DurationMs = durationMs;
        }
    }
}

// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

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

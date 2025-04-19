// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using System.Text.Json.Serialization;

namespace Awb.Core.DataPackets
{
    public class StsServoPacketData
    {
        [JsonPropertyName("Ch")]
        public required uint Channel { get; set; }

        [JsonPropertyName("TVal")]
        public required int TargetValue { get; set; }

        [JsonPropertyName("Name")]
        public string? Name { get; set; }

        [JsonPropertyName("Speed")]
        public required int Speed { get; set; }

        [JsonPropertyName("Acc")]
        public int Acc { get; set; }
    }
}
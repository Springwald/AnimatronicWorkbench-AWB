// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using System.Text.Json.Serialization;

namespace Awb.Core.DataPackets
{
    public class StsServoPacketData
    {
        [JsonPropertyName("Ch")]
        public uint Channel { get; set; }

        [JsonPropertyName("TVal")]
        public int TargetValue { get; set; }

        [JsonPropertyName("Name")]
        public string? Name { get; set; }
    }
}
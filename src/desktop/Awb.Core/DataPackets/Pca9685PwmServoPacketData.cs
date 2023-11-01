// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using System.Text.Json.Serialization;

namespace Awb.Core.DataPackets
{
    public class Pca9685PwmServoPacketData
    {
        [JsonPropertyName("Ch")]
        public uint Channel { get; set; }

        [JsonPropertyName("I2c")]
        public uint I2cAddress { get; set; }

        [JsonPropertyName("TVal")]
        public int TargetValue { get; set; }

        [JsonPropertyName("Name")]
        public string? Name { get; set; }
    }
}

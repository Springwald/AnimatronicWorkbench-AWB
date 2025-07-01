// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

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

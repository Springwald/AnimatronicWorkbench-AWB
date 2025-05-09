// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using System.Text.Json.Serialization;

namespace Awb.Core.DataPackets
{
    public class DataPacketContent
    {
        [JsonPropertyName("ReadValue")]
        public ReadValueData? ReadValue { get; set; }

        [JsonPropertyName("DispMsg")]
        public DisplayMessage? DisplayMessage { get; set; }

        [JsonPropertyName("Pca9685Pwm")]
        public Pca9685PwmServosPacketData? Pca9685PwmServos { get; set; }

        [JsonPropertyName("STS")]
        public StsServosPacketData? StsServos { get; set; }
        [JsonPropertyName("SCS")]
        public StsServosPacketData? ScsServos { get; set; }
    }
}

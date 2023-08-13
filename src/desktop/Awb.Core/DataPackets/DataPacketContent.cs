// AnimatronicWorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using System.Text.Json.Serialization;

namespace Awb.Core.DataPackets
{
    public class DataPacketContent
    {
        [JsonPropertyName("DispMsg")]
        public DisplayMessage? DisplayMessage { get; set; }

        [JsonPropertyName("AdfPwm")]
        public AdafruitPwm[]? PwmServos { get; set; }

        [JsonPropertyName("STS")]
        public StsServosPacketData? StsServos { get; set; }
    }
}

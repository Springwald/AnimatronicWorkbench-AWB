// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using System.Text.Json.Serialization;

namespace Awb.Core.DataPackets.ResponseDataPackets
{
    public class ReadValueResponseDataPacket
    {
        [JsonPropertyName("StsServo")]
        public FeetechServoReadValueResult? StsServo { get; set; }

        [JsonPropertyName("ScsServo")]
        public FeetechServoReadValueResult? ScsServo { get; set; }
    }
}

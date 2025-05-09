// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using System.Text.Json.Serialization;

namespace Awb.Core.DataPackets
{
    public class ReadValueData
    {
        public struct TypeNames
        {
            public const string ScsServo = "ScsServo";
            public const string StsServo = "StsServo";
        }

        [JsonPropertyName("TypeName")]
        public string TypeName { get; set; }

        [JsonPropertyName("Id")]
        public string Id { get; set; }

        public ReadValueData(string typeName, string id)
        {
            this.TypeName = typeName;
            this.Id = id;
        }

    }
}

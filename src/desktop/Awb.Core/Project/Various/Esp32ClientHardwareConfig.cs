// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Awb.Core.Project.Various
{
    public class Esp32ClientHardwareConfig : IProjectObjectListable
    {
        [DisplayName("Client ID")]
        [Description("The ID of this ESP32 client device.")]
        [Range(1, 254)]
        public required uint ClientId { get; set; } = 1;

        public IEnumerable<ProjectProblem> GetProblems(AwbProject project)
        {
            yield break;
        }

        [JsonIgnore]
        public string TitleShort => "ESP32 client hardware";

        [JsonIgnore]
        public string TitleDetailled => TitleShort;
    }
}

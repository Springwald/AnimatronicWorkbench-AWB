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
    public class InputConfig : IProjectObjectListable
    {
        public required int Id { get; set; }

        [DisplayName("Client ID")]
        [Description("The ID of the AWB client device that controls this servo.")]
        [Range(1, 254)]
        public required uint ClientId { get; set; } = 1;

        [DisplayName("Title")]
        [Description("A descriptive title for this input like 'test-mode'.")]
        public required string Title { get; set; }

        [DisplayName("IO hardware pin if input is GPIO input")]
        public int? IoPin { get; set; }

        [JsonIgnore]
        public string TitleShort => Title ?? $"no title for input '{Id}'";

        [JsonIgnore]
        public string TitleDetailed => $"Input {TitleShort}";

        public IEnumerable<ProjectProblem> GetContentProblems(AwbProject project)
        {
            yield break;
        }

        public InputConfig(int id, string title)
        {
            Id = id;
            Title = title;
        }
    }
}

// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Awb.Core.Project.Clients;

namespace Awb.Core.Project.Various
{
    public class InputConfig : IProjectObjectListable
    {
        public required int Id { get; set; }

        [Display(Name = "Title", GroupName = "General", Order = 1)]
        [Description("A descriptive title for this input like 'test-mode'.")]
        public required string Title { get; set; }

        [Display(Name = "Client ID", GroupName = "General", Order = 2)]
        [Description("The ID of the AWB client device that controls this servo.")]
        [Range(1, 254)]
        public required uint ClientId { get; set; } = 1;

        [DisplayName("IO hardware pin if input is GPIO input")]
        [Range(1, Esp32.MaxGpIoPortNumber)]
        public int? IoPin { get; set; }

        [JsonIgnore]
        public string TitleShort => String.IsNullOrWhiteSpace(Title) ? $"Input has no title set '{Id}'" : Title;


        [JsonIgnore]
        public string TitleDetailed => $"Input {TitleShort}";

        public IEnumerable<ProjectProblem> GetContentProblems(AwbProject project)
        {
            yield break;
        }
    }
}

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
    public class TimelineState : IProjectObjectListable
    {
        public required int Id { get; set; }

        [Display(Name = "Title", GroupName = "General", Order = 1)]
        public required string Title { get; set; }

        [DisplayName("Export")]
        [Description("Export timelines with this state to the client project source code")]
        public bool Export { get; set; } = true;

        [DisplayName("Autoplay")]
        [Description("Play the timeline automatically when the state is active.\r\nIf false: The timeline will be played when the user activates it manually by remote control")]
        public bool AutoPlay { get; set; } = true;

        /// <summary>
        /// the companion property for the PositiveInputsAsString property.
        /// needed for json serialization
        /// </summary>
        public int[] PositiveInputs { get; set; } = Array.Empty<int>();

        /// <summary>
        /// the companion property for the NegativeInputsAsString property.
        /// needed for json serialization
        /// </summary>
        public int[] NegativeInputs { get; set; } = Array.Empty<int>();

        [DisplayName("Positive inputs IDs")]
        [Description("The state is only available when one of this inputs is on.\r\nFormat: Use commas to list multiple IDs.")]
        [RegularExpression(@"(\d{1,4}(,\d{1,4})*)?", ErrorMessage = "Please enter a comma separated list of integer input IDs.")]
        [JsonIgnore]
        public string? PositiveInputsAsString
        {
            get => StringFromIntArray(PositiveInputs);
            set => PositiveInputs = IntArrayFromString(value);
        }

        [DisplayName("Negative inputs IDs")]
        [Description("The state is NOT available when one of this inputs is on.\r\nFormat: Use commas to list multiple IDs.")]
        [RegularExpression(@"(\d{1,4}(,\d{1,4})*)?", ErrorMessage = "Please enter a comma separated list of integer input IDs.")]
        [JsonIgnore]
        public string? NegativeInputsAsString
        {
            get => StringFromIntArray(NegativeInputs);
            set => NegativeInputs = IntArrayFromString(value);
        }

        public IEnumerable<ProjectProblem> GetContentProblems(AwbProject project)
        {
            // check if the positive and negative inputs are existing in the project
            foreach (var inputId in PositiveInputs.Concat(NegativeInputs))
            {
                if (project.Inputs.All(x => x.Id != inputId))
                {
                    yield return new ProjectProblem
                    {
                        ProblemType = ProjectProblem.ProblemTypes.Error,
                        Message = $"Input with ID {inputId} not found in project.",
                        Source = Title,
                    };

                }
            }

            if (PositiveInputs?.Length > 1)
                yield return new ProjectProblem
                {
                    ProblemType = ProjectProblem.ProblemTypes.Error,
                    Message = $"TimelineState [{Id}] {Title} has more than 1 positive input. Actually only 1 is supported.",
                    Source = Title,
                };

            if (NegativeInputs?.Length > 1)
                yield return new ProjectProblem
                {
                    ProblemType = ProjectProblem.ProblemTypes.Error,
                    Message = $"TimelineState [{Id}] {Title} has more than 1 negative input. Actually only 1 is supported.",
                    Source = Title,
                };
        }

        [JsonIgnore]
        public string TitleShort => String.IsNullOrWhiteSpace(Title) ? $"TimelineState has no title set '{Id}'" : Title;

        [JsonIgnore]
        public string TitleDetailed => $"TimelineState \"{TitleShort}\" (ID {Id})";

        private string StringFromIntArray(int[] value)
            => string.Join(",", value.Select(x => x.ToString()));

        private int[] IntArrayFromString(string? positiveInputsAsString)
        {
            if (string.IsNullOrWhiteSpace(positiveInputsAsString))
            {
                return Array.Empty<int>();
            }
            return positiveInputsAsString.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(x => int.Parse(x)).ToArray();
        }
    }
}
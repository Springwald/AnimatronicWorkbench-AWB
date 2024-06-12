// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Awb.Core.Project.Servos
{
    public abstract class FeetechBusServoConfig : IDeviceConfig, IProjectObjectListable
    {
        public required string Id { get; set; }

        [DisplayName("Client ID")]
        [Description("The ID of the AWB client device that controls this servo.")]
        [Range(1, 254)]
        public required uint ClientId { get; set; } = 1;

        [DisplayName("Servo ID")]
        [Description("The ID of the servo on the servo bus (1-254).")]
        [Range(1, 254)]
        public required uint Channel { get; set; }

        [DisplayName("Title")]
        [Description("A descriptive title for this servo like 'left-upper eyelid'.")]
        public required string Title { get; set; }

        /// <summary>
        /// The companion property for the RelaxRangesAsString property.
        /// Needed for json serialization.
        /// </summary>
        public ServoRelaxRange[] RelaxRanges { get; set; } = Array.Empty<ServoRelaxRange>();

        [Display]
        [Description("When the servo is some seconds unchanged and inside this ranges, the servo power will turned off.\r\nFormat:2000-2200\r\n Use commas to list multiple ranges.")]
        [RegularExpression(@"(\d{1,4}[-]\d{1,4},?)*")]
        [JsonIgnore]
        public string? RelaxRangesAsString
        {
            get => ServoRelaxRange.ToString(RelaxRanges);
            set => RelaxRanges = ServoRelaxRange.FromString(value);
        }

        [DisplayName("Global fault")]
        [Description("If this servo is in fault state (e.g.  overheat, overtorque, etc.) should all actuators be deactivated or only this one?")]

        public bool GlobalFault { get; set; }

        [DisplayName("Lowest value")]
        [Description("The value when the servo curve is at its lowest point. Possibly confusing: Can be greater than the value for 'high'.")]
        public abstract int MinValue { get; set; }

        [DisplayName("Highest value")]
        [Description("The value when the servo curve is at its highest point. Possibly confusing: Can be greater than the value for 'low'.")]
        public abstract int MaxValue { get; set; }

        [DisplayName("Default value")]
        [Description("Must be between the highest and lowest value.")]
        public abstract int? DefaultValue { get; set; }

        [DisplayName("Speed")]
        public abstract int? Speed { get; set; }

        public abstract IEnumerable<ProjectProblem> GetContentProblems(AwbProject project);

        protected IEnumerable<ProjectProblem> GetBaseProblems(AwbProject project)
        {
            // check if the default value is between the min and max value
            if (DefaultValue < Math.Min(MinValue,MaxValue) || DefaultValue > Math.Max(MinValue, MaxValue))
                yield return new ProjectProblem
                {
                    ProblemType = ProjectProblem.ProblemTypes.Error,
                    Message = $"The default value '{DefaultValue}' is not between the lowest value '{MinValue}' and the highest value '{MaxValue}' for servo '{TitleShort}'",
                    Source = TitleDetailed,
                    Category = ProjectProblem.Categories.Servo
                };

            // check if the relax ranges are valid
            foreach (var relaxRange in RelaxRanges)
            {
                if (relaxRange.MinValue > relaxRange.MaxValue)
                    yield return new ProjectProblem
                    {
                        ProblemType = ProjectProblem.ProblemTypes.Error,
                        Message = $"The relax range '{relaxRange}' first value is lower than second value for servo '{TitleShort}'",
                        Source = TitleDetailed,
                        Category = ProjectProblem.Categories.Servo
                    };
            }

            yield break;
        }

        [JsonIgnore]
        public string TitleShort => Title ?? $"StsServo has no title set '{Id}'";

        [JsonIgnore]
        public string TitleDetailed => $"StsServo '{TitleShort}' (Id: {Id}, ClientId: {ClientId}, Channel: {Channel})";
    }
}

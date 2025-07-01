// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Awb.Core.Project.Servos
{
    public class DynamixelBusServoConfig : IDeviceConfig, IProjectObjectListable
    {
        public required string Id { get; set; }

        [Display(Name = "Title", GroupName = "General", Order = 1)]
        [Description("A descriptive title for this servo like 'left-upper eyelid'.")]
        public required string Title { get; set; }


        [Display(Name = "Client ID", GroupName = "General", Order = 2)]
        [Description("The ID of the AWB client device that controls this servo.")]
        [Range(1, 254)]
        public required uint ClientId { get; set; } = 1;

        [Display(Name = "Servo ID", GroupName = "General", Order = 3)]
        [Description("The ID of the servo on the servo bus (1-254).")]
        [Range(1, 254)]
        public required uint Channel { get; set; }

        [Display(Name = "Global fault", GroupName = "General", Order = 4)]
        [Description("If this servo is in fault state (e.g.  overheat, overtorque, etc.) should all actuators be deactivated or only this one?")]
        public bool GlobalFault { get; set; }

        /// <summary>
        /// The companion property for the RelaxRangesAsString property.
        /// Needed for json serialization.
        /// </summary>
        public ServoRelaxRange[] RelaxRanges { get; set; } = Array.Empty<ServoRelaxRange>();

        [Display(Name = "Relax-range", GroupName = "Values", Order = 1)]
        [Description("When the servo is some seconds unchanged and inside this ranges, the servo power will turned off.\r\nFormat:2000-2200\r\n Use commas to list multiple ranges.")]
        [RegularExpression(@"(\d{1,4}[-]\d{1,4},?)*")]
        [JsonIgnore]
        public string? RelaxRangesAsString
        {
            get => ServoRelaxRange.ToString(RelaxRanges);
            set => RelaxRanges = ServoRelaxRange.FromString(value);
        }

        [Display(Name = "Max Temperature", GroupName = "Values", Order = 2)]
        [Description("If the servo temperature is above this value, the servo will be deactivated.")]
        [Range(20, 60)]
        public uint MaxTemp { get; set; } = 55;

        [Display(Name = "Max Torque", GroupName = "Values", Order = 3)]
        [Description("If the servo torque is above this value, the servo will be deactivated. Negative torque values are treated as positive.")]
        [Range(100, 1000)]
        public uint MaxTorque { get; set; } = 400;

        [Display(Name = "Lowest value", GroupName = "Values", Order = 4)]
        [Description("The value when the servo curve is at its lowest point. Possibly confusing: Can be greater than the value for 'high'.")]
        public int MinValue { get; set; }

        [Display(Name = "Highest value", GroupName = "Values", Order = 5)]
        [Description("The value when the servo curve is at its highest point. Possibly confusing: Can be greater than the value for 'low'.")]
        public int MaxValue { get; set; }

        [Display(Name = "Default value", GroupName = "Values", Order = 6)]
        [Description("Must be between the highest and lowest value.")]
        public int? DefaultValue { get; set; }

        [Display(Name = "Speed value", GroupName = "Values", Order = 7)]
        public int? Speed { get; set; }

        public IEnumerable<ProjectProblem> GetContentProblems(AwbProject project)
        {

            // check if the default value is between the min and max value
            if (DefaultValue < Math.Min(MinValue, MaxValue) || DefaultValue > Math.Max(MinValue, MaxValue))
                yield return new ProjectProblem
                {
                    ProblemType = ProjectProblem.ProblemTypes.Error,
                    Message = $"The default value '{DefaultValue}' is not between the lowest value '{MinValue}' and the highest value '{MaxValue}' for servo '{TitleShort}'",
                    Source = TitleDetailed,
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
                    };
            }

            yield break;
        }

        [JsonIgnore]
        public string TitleShort => String.IsNullOrWhiteSpace(Title) ? $"Dynamixel servo has no title set '{Id}'" : Title;

        [JsonIgnore]
        public string TitleDetailed => $"Dynamixel '{TitleShort}' (Id: {Id}, ClientId: {ClientId}, Channel: {Channel})";
    }
}

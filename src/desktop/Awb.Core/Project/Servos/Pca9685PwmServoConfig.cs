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
    public class Pca9685PwmServoConfig : IDeviceConfig, IProjectObjectListable
    {
        public const int MaxValConst = 4095;

        public required string Id { get; set; }

        [Display(Name = "Title", GroupName = "General", Order = 1)]
        [Description("A descriptive title for this servo like 'left-upper eyelid'.")]
        public required string Title { get; set; }

        [Display(Name = "Client ID", GroupName = "General", Order = 2)]
        [Description("The ID of the AWB client device that controls this servo.")]
        [Range(1, 254)]
        public required uint ClientId { get; set; } = 1;

        [DisplayName("I2C Address")]
        [Description("The I2C address of the PCA9685 PWM controller that controls this servo.")]
        [Range(0x40, 0x7F)]
        public required uint I2cAdress { get; set; }

        [DisplayName("Channel")]
        [Description("The channel of the PCA9685 PWM controller that controls this servo.")]
        [Range(0, 15)]
        public required uint Channel { get; set; }


        [DisplayName("Lowest value")]
        [Description("The value when the servo curve is at its lowest point. Possibly confusing: Can be greater than the value for 'high'.")]
        [Range(0, MaxValConst)]
        public int MinValue { get; set; }

        [DisplayName("Highest value")]
        [Description("The value when the servo curve is at its highest point. Possibly confusing: Can be greater than the value for 'low'.")]
        [Range(0, MaxValConst)]
        public int MaxValue { get; set; }

        [DisplayName("Default value")]
        [Description("Must be between the highest and lowest value.")]
        [Range(0, MaxValConst)]
        public int? DefaultValue { get; set; }

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
        }

        [JsonIgnore]
        public string TitleShort => String.IsNullOrWhiteSpace(Title) ? $"Pca9685PwmServo has no title set '{Id}'" : Title;

        [JsonIgnore]
        public string TitleDetailed => "Pca9685PwmServo " + TitleShort;

      
    }
}

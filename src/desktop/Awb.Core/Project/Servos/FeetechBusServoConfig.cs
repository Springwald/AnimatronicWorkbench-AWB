// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2026 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Project.Actuators;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Awb.Core.Project.Servos
{
    public abstract class FeetechBusServoConfig : IDeviceConfig, IProjectObjectListable, IServoConfig
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


        [Display(Name = "Max Temperature", GroupName = "Values", Order = 2)]
        [Description("If the servo temperature is above this value, the servo will be deactivated.")]
        [Range(20, 60)]
        public uint MaxTemp { get; set; } = 55;

        [Display(Name = "Max Torque", GroupName = "Values", Order = 3)]
        [Description("If the servo torque is above this value, the servo will be deactivated. Negative torque values are treated as positive.")]
        [Range(100, 1000)]
        public uint MaxTorque { get; set; } = 400;

        [Display(Name = "Project lowest position", GroupName = "Values", Order = 4)]
        [Description("The value when the servo curve is at its lowest point. Possibly confusing: Can be greater than the value for 'high'.")]
        [SupportsTakeOverTheCurrentServoValue]
        public abstract int MinValue { get; set; }

        [Display(Name = "Project highest position", GroupName = "Values", Order = 5)]
        [Description("The value when the servo curve is at its highest point. Possibly confusing: Can be greater than the value for 'low'.")]
        [SupportsTakeOverTheCurrentServoValue]
        public abstract int MaxValue { get; set; }

        [Display(Name = "Default value", GroupName = "Values", Order = 6)]
        [Description("Must be between the highest and lowest value.")]
        [SupportsTakeOverTheCurrentServoValue]
        public abstract int? DefaultValue { get; set; }

        public abstract IEnumerable<ProjectProblem> GetContentProblems(AwbProject project);

        protected abstract IEnumerable<ProjectProblem> GetBaseProblems(AwbProject project);

        [JsonIgnore]
        public string TitleShort => String.IsNullOrWhiteSpace(Title) ? $"StsServo has no title set '{Id}'" : Title;

        [JsonIgnore]
        public string TitleDetailed => $"StsServo '{TitleShort}' (Id: {Id}, ClientId: {ClientId}, Channel: {Channel})";

        [JsonIgnore]
        public bool CanReadServoPosition => true;
    }
}

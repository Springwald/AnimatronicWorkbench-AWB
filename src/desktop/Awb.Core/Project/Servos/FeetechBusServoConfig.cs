// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
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
   
        [DisplayName("Global fault")]
        [Description("If this servo is in fault state (e.g.  overheat, overtorque, etc.) should all actuators be deactivated or only this one?")]
        public bool GlobalFault { get; set; }

        [JsonIgnore]
        public string TitleShort => Title ?? $"StsServo has no title set '{Id}'";

        [JsonIgnore]
        public string TitleDetailled => $"StsServo '{TitleShort}' (Id: {Id}, ClientId: {ClientId}, Channel: {Channel})";
    }
}

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
    public class StsFeetechServoConfig : FeetechBusServoConfig
    {
        [DisplayName("Lowest value")]
        [Description("The value when the servo curve is at its lowest point. Possibly confusing: Can be greater than the value for 'high'.")]
        [Range(0, 4095)]
        public int MinValue { get; set; }

        [DisplayName("Highest value")]
        [Description("The value when the servo curve is at its highest point. Possibly confusing: Can be greater than the value for 'low'.")]
        [Range(0, 4095)]
        public int MaxValue { get; set; }

        [DisplayName("Default value")]
        [Description("Must be between the highest and lowest value.")]
        [Range(0, 4095)]
        public int? DefaultValue { get; set; }

        [DisplayName("Acceleration")]
        [Description("Set the start/stop acceleration. The smaller the value, the lower the acceleration. The maximum value that can be set is 150.")]
        [Range(0, 150)]
        public int? Acceleration { get; set; }

        [DisplayName("Speed")]
        [Description("The speed is the number of steps per second, 50 steps/sec≈0.732RPM. 0=Max speed")]
        [Range(0, 3073)]
        public int? Speed { get; set; }



    }
}

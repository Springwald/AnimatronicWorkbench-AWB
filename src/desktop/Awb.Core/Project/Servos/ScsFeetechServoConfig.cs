// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Awb.Core.Project.Servos
{
    public class ScsFeetechServoConfig : FeetechBusServoConfig
    {
        [DisplayName("Lowest value")]
        [Description("The value when the servo curve is at its lowest point. Possibly confusing: Can be greater than the value for 'high'.")]
        [Range(0, 1023)]
        public int MinValue { get; set; }

        [DisplayName("Highest value")]
        [Description("The value when the servo curve is at its highest point. Possibly confusing: Can be greater than the value for 'low'.")]
        [Range(0, 1023)]
        public int MaxValue { get; set; }

        [DisplayName("Default value")]
        [Description("Must be between the highest and lowest value.")]
        [Range(0, 1023)]
        public int? DefaultValue { get; set; }

        [DisplayName("Speed")]
        [Description("The speed is the number of steps per second, 50 steps/sec≈2.928RPM. 0=Max speed")] 
        [Range(0, 3073)]
        public int? Speed { get; set; }

    }
}

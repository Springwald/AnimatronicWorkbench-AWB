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
        [Range(0, 1023)]
        public override int MinValue { get; set; }

        [Range(0, 1023)]
        public override int MaxValue { get; set; }

        [Range(0, 1023)]
        public override int? DefaultValue { get; set; }

        [Description("The speed is the number of steps per second, 50 steps/sec≈2.928RPM. 0=Max speed")] 
        [Range(0, 3073)]
        public override int? Speed { get; set; }
    }
}

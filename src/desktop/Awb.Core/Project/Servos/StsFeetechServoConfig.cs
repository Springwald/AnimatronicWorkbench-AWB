// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Awb.Core.Project.Servos
{
    public class StsFeetechServoConfig : FeetechBusServoConfig
    {
        public const int MaxValueConst = 4095;

        public const int MaxSpeedConst = 4000;
        public const string SpeedDescriptionConst = "The speed is the number of steps per second, 50 steps/sec≈0.732RPM. 0=Max speed";

        public const int MaxAccConst = 150;
        public const string AccDescriptionConst = "Set the start/stop acceleration. The smaller the value, the lower the acceleration. The maximum value that can be set is 150.";


        [Range(0, MaxValueConst)]
        public override int MinValue { get; set; }

        [Range(0, MaxValueConst)]
        public override int MaxValue { get; set; }

        [Range(0, MaxValueConst)]
        public override int? DefaultValue { get; set; }

        [Description(SpeedDescriptionConst)]
        [Range(-1, MaxSpeedConst)]
        public override int? Speed { get; set; } = 2000;

        [DisplayName("Acceleration")]
        [Description(AccDescriptionConst + "\r\n-1 to use STS servo default acceleration.")]
        [Range(0, MaxAccConst)]
        public int? Acceleration { get; set; } = 100;

        public override IEnumerable<ProjectProblem> GetContentProblems(AwbProject project)
        {
            foreach (var item in GetBaseProblems(project)) yield return item;

            // add STS specific checks here
        }
    }
}

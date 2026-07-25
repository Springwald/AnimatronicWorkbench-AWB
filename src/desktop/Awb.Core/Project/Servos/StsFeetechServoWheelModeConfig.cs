// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2026 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Awb.Core.Project.Servos
{
    public class StsFeetechServoWheelModeConfig : FeetechBusServoConfig
    {
        public const int MinSpeedConst = -3500;
        public const int MaxSpeedConst = 3500;
        public const string SpeedDescriptionConst = "The speed is the number of steps per second, 50 steps/sec≈0.732RPM. 0=Max speed";

        public const int MaxAccConst = 150;
        public const string AccDescriptionConst = "Set the start/stop acceleration. The smaller the value, the lower the acceleration. The maximum value that can be set is 150.";


        [Range(MinSpeedConst, MaxSpeedConst)]
        public override int MinValue { get; set; }

        [Range(MinSpeedConst, MaxSpeedConst)]
        public override int MaxValue{ get; set; }

        [Range(MinSpeedConst, MaxSpeedConst)]
        public override int? DefaultValue { get; set; }

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

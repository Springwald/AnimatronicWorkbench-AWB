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
        private const int maxValConst = 1023;
        public const int MaxSpeedConst = 1500;
        public const string SpeedDescriptionConst = "The speed is the number of steps per second.\r\n50 steps/sec≈2.928RPM. 0=Max speed";


        [Range(0, maxValConst)]
        public override int MinValue { get; set; }

        [Range(0, maxValConst)]
        public override int MaxValue { get; set; }

        [Range(0, maxValConst)]
        public override int? DefaultValue { get; set; }

        [Description(SpeedDescriptionConst + "\r\n-1 to use SCS servo default value.")]
        [Range(-1, MaxSpeedConst)]
        public override int? Speed { get; set; }

        public override IEnumerable<ProjectProblem> GetContentProblems(AwbProject project)
        {
            foreach (var item in GetBaseProblems(project)) yield return item;

            // add SCS specific checks heres
        }
    }
}

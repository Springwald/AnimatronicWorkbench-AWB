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
        [Range(0, 4095)]
        public override int MinValue { get; set; }

        [Range(0, 4095)]
        public override int MaxValue { get; set; }

        [Range(0, 4095)]
        public override int? DefaultValue { get; set; }

        [Description("The speed is the number of steps per second, 50 steps/sec≈0.732RPM. 0=Max speed")]
        [Range(0, 3073)]
        public override int? Speed { get; set; }

        [DisplayName("Acceleration")]
        [Description("Set the start/stop acceleration. The smaller the value, the lower the acceleration. The maximum value that can be set is 150.")]
        [Range(0, 150)]
        public int? Acceleration { get; set; }

        public override IEnumerable<ProjectProblem> GetContentProblems(AwbProject project)
        {
            foreach (var item in GetBaseProblems(project)) yield return item;

            // add STS specific checks here
        }
    }
}

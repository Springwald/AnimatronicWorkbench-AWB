// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License


using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Awb.Core.Project.Servos
{
    public class ScsBusConfig : IProjectObjectListable
    {
        /* SCS serial servo bus settings */
        
        [DisplayName("servo default speed")]
        [Description(ScsFeetechServoConfig.SpeedDescriptionConst)]
        [Range(0, ScsFeetechServoConfig.MaxSpeedConst)]
        public int ScsServoSpeed { get; set; } = 500;

        [DisplayName("RXD pin")]
        [Description("eg. GPIO 18 for waveshare servo driver")]
        [Range(1, Esp32.MaxGpIoPortNumber)]
        public int ScsServoRxd { get; set; } = 18;

        [DisplayName("TXD pin")]
        [Description("eg. GPIO 19 for waveshare servo driver")]
        [Range(1, Esp32.MaxGpIoPortNumber)]
        public int ScsServoTxd { get; set; } = 19;

        [DisplayName("servo max temperature")]
        [Description("max temperature (celsius) before servo is disabled")]
        [Range(20, 70)]
        public int ScsServoMaxTemp { get; set; } = 55;

        [DisplayName("servo max load")]
        [Description("max load before servo is disabled")]
        [Range(0, 4096)]
        public int ScsServoMaxLoad { get; set; } = 600;

        public string TitleShort => "SCS serial bus adapter";

        public string TitleDetailed => TitleShort;

        public IEnumerable<ProjectProblem> GetContentProblems(AwbProject project)
        {
            throw new NotImplementedException();
        }
    }
}

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
    public class StsBusConfig : IProjectObjectListable
    {
        /* STS serial servo settings */

        [DisplayName("servo default speed")]
        [Description(StsFeetechServoConfig.SpeedDescriptionConst)]
        [Range(-1, StsFeetechServoConfig.MaxSpeedConst)]
        public int StsServoSpeed { get; set; } = 1500;

        [DisplayName("servo default acceleration")]
        [Description(StsFeetechServoConfig.AccDescriptionConst)]
        [Range(-1, StsFeetechServoConfig.MaxAccConst)]
        public int StsServoAcceleration { get; set; } = 100;

        [DisplayName("RXD pin")]
        [Description("eg. GPIO 18 for waveshare servo driver")]
        [Range(1, Esp32.MaxGpIoPortNumber)]
        public int StsServoRxd { get; set; } = 18;

        [DisplayName("TXD pin")]
        [Description("eg. GPIO 19 for waveshare servo driver")]
        [Range(1, Esp32.MaxGpIoPortNumber)]
        public int StsServoTxd { get; set; } = 19;

        [DisplayName("servo max temperature")]
        [Description("max temperature (celsius) before servo is disabled")]
        [Range(20, 70)]
        public int StsServoMaxTemp { get; set; } = 55;

        [DisplayName("servo max load")]
        [Description("max load before servo is disabled")]
        [Range(0, 4096)]
        public int? StsServoMaxLoad { get; set; } = 400;

        /* autoplay state selector */
        // if a servo position feedback is used as a state selector, define the servo channel here.
        // if you don't use a servo as state selector, set this to -1 or undefine it
        // #define AUTOPLAY_STATE_SELECTOR_STS_SERVO_CHANNEL 9
        // if the servo position feedback is not exatly 0 at the first state, define the offset here (-4096 to 4096)
        //#define AUTOPLAY_STATE_SELECTOR_STS_SERVO_POS_OFFSET 457

        public string TitleShort => "STS serial bus adapter";

        public string TitleDetailed => TitleShort;

        public IEnumerable<ProjectProblem> GetContentProblems(AwbProject project)
        {
            yield break;
        }
    }
}

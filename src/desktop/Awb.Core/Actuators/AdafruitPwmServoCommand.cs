// AnimatronicWorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

namespace Awb.Core.Actuators
{
    public class AdafruitPwmServoCommand
    {
        public int Channel { get; set; }
        public int I2cAdress { get; set; }
        public int? TargetValue { get; set; }
    }
}

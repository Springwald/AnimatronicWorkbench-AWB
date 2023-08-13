// AnimatronicWorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

namespace Awb.Core.DataPackets
{
    public class AdafruitPwm
    {
        public int I2c { get; set; }

        public AdafruitPwmChannel[]? Channels { get; set; }

        public AdafruitPwm(int i2cAddress)
        {
            I2c = i2cAddress;
        }
    }
}

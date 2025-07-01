// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

namespace Awb.Core.DataPackets
{
    public class Pca9685Pwm
    {
        public int I2c { get; set; }

        public AdafruitPwmChannel[]? Channels { get; set; }

        public Pca9685Pwm(int i2cAddress)
        {
            I2c = i2cAddress;
        }
    }
}

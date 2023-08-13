// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

namespace Awb.Core.Configs
{
    public class AdafruitPwmServoConfig : IDeviceConfig
    {
        public string Id { get; set; }
        public uint ClientId { get; set; }
        public int I2cAdress { get; set; }
        public int Channel { get; set; }

        public string? Name { get; set; }
        public int MinValue { get; set; }
        public int MaxValue { get; set; }
        public int? DefaultValue { get; set; }
    }
}

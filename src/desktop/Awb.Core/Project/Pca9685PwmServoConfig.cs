// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

namespace Awb.Core.Configs
{
    public class Pca9685PwmServoConfig : IDeviceConfig
    {
        public string Id { get; set; }
        public uint ClientId { get; set; }
        public uint I2cAdress { get; set; }
        public uint Channel { get; set; }
        public string? Name { get; set; }
        public int MinValue { get; set; }
        public int MaxValue { get; set; }
        public int? DefaultValue { get; set; }
        public int? Acceleration { get; set; }
        public int? Speed { get; set; }

        public Pca9685PwmServoConfig(string id, uint clientId, uint i2cAdress,  uint channel)
        {
            Id = id;
            ClientId = clientId;
            I2cAdress = i2cAdress;
            Channel = channel;
        }
    }
}

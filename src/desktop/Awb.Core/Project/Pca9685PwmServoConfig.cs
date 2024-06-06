// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

namespace Awb.Core.Project
{
    public class Pca9685PwmServoConfig : IDeviceConfig, IProjectObjectListable
    {
        public string Id { get; set; }
        public uint ClientId { get; set; }
        public uint I2cAdress { get; set; }
        public uint Channel { get; set; }
        public string Title { get; set; }
        public int MinValue { get; set; }
        public int MaxValue { get; set; }
        public int? DefaultValue { get; set; }

        public string TitleShort => Title ?? $"no title for Pca9685PwmServo '{Id}'";

        public string TitleDetailled => "Pca9685PwmServo " + TitleShort;

        public Pca9685PwmServoConfig(string id, uint clientId, uint i2cAdress, uint channel, string title)
        {
            Id = id;
            ClientId = clientId;
            I2cAdress = i2cAdress;
            Channel = channel;
            Title = title;
        }
    }
}

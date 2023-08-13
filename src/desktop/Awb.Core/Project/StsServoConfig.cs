// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

namespace Awb.Core.Configs
{
    public class StsServoConfig : IDeviceConfig
    {
        public string Id { get; set; }

        public uint ClientId { get; set; }
        public uint Channel { get; set; }
        public string? Name { get; set; }
        public int MinValue { get; set; }
        public int MaxValue { get; set; }
        public int? DefaultValue { get; set; }
        public int? Accelleration { get; set; }
        public int? Speed { get; set; }

        public StsServoConfig(string id, uint clientId, uint channel)
        {
            this.Id = id;
            this.ClientId = clientId;
            this.Channel = channel;
        }
    }
}

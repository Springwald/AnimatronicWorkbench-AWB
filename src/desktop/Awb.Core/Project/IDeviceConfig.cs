// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

namespace Awb.Core.Configs
{
    internal interface IDeviceConfig
    {
        /// <summary>
        /// If the device can be paused, this is the switch id to pause the device
        /// </summary>
        public int? PauseSwitchId { get; set; }

        /// <summary>
        /// the unqiue id of the device
        /// the unqiue id of the device
        /// </summary>
        public string Id { get; set; }
    }
}

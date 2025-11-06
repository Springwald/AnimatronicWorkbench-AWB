// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

namespace Awb.Core.Project
{
    public interface IDeviceConfig
    {
        /// <summary>
        /// the unqiue id of the device
        /// </summary>
        public string Id { get; set; }
    }
}

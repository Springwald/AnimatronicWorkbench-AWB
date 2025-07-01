// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

namespace Awb.Core.Project.Servos
{
    public interface IServoConfig
    {
        /// <summary>
        /// Is this servo able to report its position?
        /// </summary>
        bool CanReadServoPosition { get; }
    }
}

// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

namespace Awb.Core.Actuators
{
    public interface IActuator : IDisposable
    {
        /// <summary>
        /// The unique ID of the actuator
        /// </summary>
        string Id { get; }

        /// <summary>
        /// The optional visible name of the actuator
        /// </summary>
        string Name { get; }

        /// <summary>
        /// When multiple clients are used, this is the ID of the client that is currently controlling the actuator
        /// </summary>
        uint ClientId { get; }

        /// <summary>
        /// Has the target value to be send as update to the client?
        /// </summary>
        bool IsDirty { get; set; }
    }
}

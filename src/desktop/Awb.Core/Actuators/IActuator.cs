// AnimatronicWorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

namespace Awb.Core.Actuators
{
    public interface IActuator : IDisposable
    {
        string Id { get; }
        string? Name { get; }
        uint ClientId { get; }
        string Label { get; }

        /// <summary>
        /// Has the target value to be send as update to the client?
        /// </summary>
        bool IsDirty { get; set; }
    }
}

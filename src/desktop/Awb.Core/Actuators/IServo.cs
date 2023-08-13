// AnimatronicWorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

namespace Awb.Core.Actuators
{
    public interface IServo : IActuator
    {
        int MinValue { get; }
        int MaxValue { get; }
        int DefaultValue { get; }

        int TargetValue { get; set; }

        bool TurnOff();
    }
}

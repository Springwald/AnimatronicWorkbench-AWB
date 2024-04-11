// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

namespace Awb.Core.Actuators
{
    /// <summary>
    /// A servo motor, maybe a classic RC servo controlled by a PWM signal or a serial servo
    /// </summary>
    public interface IServo : IActuator
    {
        /// <summary>
        /// The minimum value this servo should handle in the constructred animatronic figure
        /// </summary>
        int MinValue { get; }

        /// <summary>
        /// The maximum value this servo should handle in the constructred animatronic figure
        /// </summary>
        int MaxValue { get; }

        /// <summary>
        /// The "normal" startup value of this servo
        /// </summary>
        int DefaultValue { get; }

        /// <summary>
        /// The requested target value of this servo
        /// </summary>
        int TargetValue { get; set; }

        /// <summary>
        /// turns the servo off. If the servo is a classic RC servo, this means that the PWM signal is set to 0.
        /// If the servo is a serial servo, this means that the servo should be complete turned off whithout any holding torque. 
        /// </summary>
        /// <returns>
        /// true if the servo was turned off, false if it was not possible to turn the servo off
        /// </returns>

        bool TurnOff();
    }
}

// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2026 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Project.Servos;
using Awb.Core.Tools;

namespace Awb.Core.Actuators
{
    /// <summary>
    /// A STS or SCS serial servo motor e.g. from the manufacturer "Wavewshare" or "Feetech" 
    /// </summary>
    public class StsScsServo : IServo
    {
        public enum StsScsTypes
        {
            Scs,
            Sts,
            StsWheelMode
        }

        /// <summary>
        /// The requested target value of this servo
        /// </summary>
        private int _targetValue;

        public StsScsTypes StsScsType { get; private set; }

        /// <summary>
        /// The unique id of this servo
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// The unique id of the client this servo is connected to
        /// </summary>
        public uint ClientId { get; private set; }

        /// <summary>
        /// The optional visible name of the servo
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// The channel of the servo, mostly starting with 1 instead of 0
        /// </summary>
        public uint Channel { get; private set; }

        public bool WheelMode => StsScsType == StsScsTypes.StsWheelMode;

        /// <summary>
        /// The maximum value this servo should handle in the constructred animatronic figure
        /// </summary>
        public int MinValue { get; private set; }

        /// <summary>
        /// The speed of the servo 
        /// </summary>
        public int? Speed { get; private set; }

        /// <summary>
        /// The Acceleration of the servo
        /// </summary>
        public int? Acceleration { get; private set; }

        /// <summary>
        /// The maximum value this servo should handle in the constructred animatronic figure
        /// </summary>
        public int MaxValue { get; private set; }

        /// <summary>
        /// The "normal" startup value of this servo
        /// </summary>
        public int DefaultValue { get; private set; }

        /// <summary>
        /// The requested target value of this servo
        /// </summary>
        public int TargetValue
        {
            get => _targetValue;
            set
            {
                if (value != _targetValue)
                {
                    _targetValue = value;
                    IsDirty = true;
                }
            }
        }

        /// <summary>
        /// Indicates if the servo has changed since the last call>
        /// </summary>
        public bool IsDirty { get; set; }

        public string Label => $"{(ClientId == 1 ? string.Empty : $"C{ClientId}-")}{StsScsType.ToString().ToUpper()}{Channel} {Title ?? string.Empty}";

        public PercentCalculator PercentCalculator { get; private set; }

        public bool IsControllerTuneable => true;

        public StsScsServo(FeetechBusServoConfig config)
        {
            Id = config.Id;
            MaxValue = config.MaxValue;
            MinValue = config.MinValue;
            ClientId = config.ClientId;
            Channel = config.Channel;
            Title = config.Title;

            IsDirty = true;

            var defaultValue = config.DefaultValue ?? config.MinValue + (config.MaxValue - config.MinValue) / 2;
            DefaultValue = defaultValue;
            TargetValue = defaultValue;

            StsScsType = config switch
            {
                StsFeetechServoConfigServoMode => StsScsTypes.Sts,
                StsFeetechServoWheelModeConfig => StsScsTypes.StsWheelMode,
                ScsFeetechServoConfig => StsScsTypes.Scs,
                _ => throw new ArgumentException("Unhandled servo config type" + config.GetType().Name)
            };

            Speed = config switch
            {
                StsFeetechServoConfigServoMode stsConfig => stsConfig.Speed ?? -1,
                StsFeetechServoWheelModeConfig wheelModeConfig => -1,
                _ => throw new ArgumentException("Unhandled servo config type" + config.GetType().Name)
            };

            Acceleration = config switch
            {
                StsFeetechServoConfigServoMode stsConfig => stsConfig.Acceleration ?? -1,
                StsFeetechServoWheelModeConfig wheelModeConfig => wheelModeConfig.Acceleration ?? -1,
                _ => throw new ArgumentException("Unhandled servo config type" + config.GetType().Name)
            };

            PercentCalculator = new PercentCalculator(MinValue, MaxValue);
        }

        public bool TurnOff()
        {
            TargetValue = -1;
            return true;
        }

        public void Dispose()
        {
            TurnOff();
        }
    }
}

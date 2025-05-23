﻿// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Project.Servos;
using Awb.Core.Tools;

namespace Awb.Core.Actuators
{
    /// <summary>
    /// A STS serial servo motor e.g. from the manufacturer "Wavewshare" or "Feebtech" 
    /// </summary>
    public class StsScsServo : IServo
    {
        public enum StsScsTypes
        {
            Scs,
            Sts
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
            // find out the StsScsType by the config type using the switch expression
            StsScsTypes type = config switch
            {
                StsFeetechServoConfig => StsScsTypes.Sts,
                ScsFeetechServoConfig => StsScsTypes.Scs,
                _ => throw new ArgumentException("Unknown servo config type")
            };

            var defaultValue = config.DefaultValue ?? config.MinValue + (config.MaxValue - config.MinValue) / 2;

            var acc = config is StsFeetechServoConfig stsFeetechServoConfig ? stsFeetechServoConfig.Acceleration : null;

            Id = config.Id;
            StsScsType = type;
            MaxValue = config.MaxValue;
            MinValue = config.MinValue;
            Speed = config.Speed;
            Acceleration = acc;
            ClientId = config.ClientId;
            Channel = config.Channel;
            Title = config.Title;
            DefaultValue = defaultValue;
            TargetValue = defaultValue;
            IsDirty = true;
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

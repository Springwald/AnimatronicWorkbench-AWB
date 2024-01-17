// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Configs;
using Awb.Core.Services;

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

        public StsScsTypes StsScsType { get; }

        /// <summary>
        /// The unique id of this servo
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The unique id of the client this servo is connected to
        /// </summary>
        public uint ClientId { get; }

        /// <summary>
        /// The optional visible name of the servo
        /// </summary>
        public string? Name { get; }

        /// <summary>
        /// The channel of the servo, mostly starting with 1 instead of 0
        /// </summary>
        public uint Channel { get; }

        /// <summary>
        /// The maximum value this servo should handle in the constructred animatronic figure
        /// </summary>
        public int MinValue { get; }

        /// <summary>
        /// The maximum value this servo should handle in the constructred animatronic figure
        /// </summary>
        public int MaxValue { get; }

        /// <summary>
        /// The "normal" startup value of this servo
        /// </summary>
        public int DefaultValue { get; }

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

        public string Label => $"[C{ClientId}-{StsScsType.ToString().ToUpper()}{Channel}] {Name ?? string.Empty}";

        public StsScsServo(StsServoConfig config, StsScsTypes type)
        {
            var defaultValue = config.DefaultValue ?? config.MinValue + (config.MaxValue - config.MinValue) / 2;

            StsScsType = type;
            Id = config.Id;
            ClientId = config.ClientId;
            Channel = config.Channel;
            Name = config.Name;
            MinValue = config.MinValue;
            MaxValue = config.MaxValue;
            DefaultValue = defaultValue;
            TargetValue = defaultValue;
            IsDirty = true;
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

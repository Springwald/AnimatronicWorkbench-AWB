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
    public class StsServo : IServo
    {
        private int _targetValue;

        public string Id { get; }
        public uint ClientId { get; }
        public string? Name { get; }
        public uint Channel { get; }
        public int MinValue { get; }
        public int MaxValue { get; }
        public int DefaultValue { get; }
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


        public bool IsDirty { get; set; }

        public string Label => $"[C{ClientId}-STS{Channel}] {Name ?? string.Empty}";

        public StsServo(StsServoConfig config, IAwbClientsService clients)
        {
            var defaultValue = config.DefaultValue ?? config.MinValue + (config.MaxValue - config.MinValue) / 2;

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

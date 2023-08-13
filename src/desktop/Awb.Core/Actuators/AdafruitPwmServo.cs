// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Clients;
using Awb.Core.Configs;
using Awb.Core.Services;

namespace Awb.Core.Actuators
{
    public class AdafruitPwmServo : IServo
    {
        private readonly Esp32ComPortClient? _espClient;
        private readonly int _channel;
        private readonly int _i2cAdress;

        public string Id { get; }
        public uint ClientId { get; }
        public string? Name { get; }
        public uint Channel { get; }
        public int MinValue { get; }
        public int MaxValue { get; }
        public int DefaultValue { get; }
        public int TargetValue { get; set; }
        public bool IsDirty { get; set; }

        public string Label => $"[C{ClientId}-PWM{Channel}] {Name ?? string.Empty}";

        public AdafruitPwmServo(AdafruitPwmServoConfig config, IAwbClientsService clients)
        {
            var defaultValue = config.DefaultValue ?? config.MinValue + (config.MaxValue - config.MinValue) / 2;

            Id = config.Id;
            ClientId = config.ClientId;
            Name = config.Name;
            MinValue = config.MinValue;
            MaxValue = config.MaxValue;
            DefaultValue = defaultValue;
            TargetValue = defaultValue;
            IsDirty = true;

            _channel = config.Channel;
            _i2cAdress = config.I2cAdress;
            _espClient = clients.GetClient(config.ClientId) ?? throw new KeyNotFoundException($"EspClientId '{config.ClientId}' not found!");
        }

        public bool TurnOff()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}

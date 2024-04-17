// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Project;
using Awb.Core.Tools;

namespace Awb.Core.Actuators
{
    public class Pca9685PwmServo : IServo
    {
        public string Id { get; }
        public uint ClientId { get; }
        public string Title { get; }
        public uint Channel { get; }
        public uint I2cAdress { get; }
        public int MinValue { get; }
        public int MaxValue { get; }
        public int DefaultValue { get; }
        public int TargetValue { get; set; }
        public bool IsDirty { get; set; }

        public string Label => $"{(ClientId == 1 ? string.Empty : $"C{ClientId}-")}PWM{Channel} {Title ?? string.Empty}";

        public PercentCalculator PercentCalculator { get; }

        public Pca9685PwmServo(Pca9685PwmServoConfig config)
        {
            var defaultValue = config.DefaultValue ?? config.MinValue + (config.MaxValue - config.MinValue) / 2;

            Id = config.Id;
            ClientId = config.ClientId;
            Title = config.Title;
            MinValue = config.MinValue;
            MaxValue = config.MaxValue;
            DefaultValue = defaultValue;
            TargetValue = defaultValue;
            IsDirty = true;

            Channel = config.Channel;
            I2cAdress = config.I2cAdress;

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

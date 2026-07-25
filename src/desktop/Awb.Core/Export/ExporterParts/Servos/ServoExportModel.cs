// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2026 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

namespace Awb.Core.Export.ExporterParts.Servos
{
    internal class ServoExportModel
    {
        /// <summary>
        /// types of supported servos
        /// </summary>
        public enum ServoExportTypes
        {
            PWM_SERVO = 0,
            STS_SERVO = 1,
            SCS_SERVO = 2,
            STSWHEEL_SERVO = 3
        }

        public required string Id { get; set; }
        public required int DefaultValue { get; set; }
        public required int Acceleration { get; set; }
        public required uint I2cAdress { get; set; }
        public required int Speed { get; set; }
        public required uint Channel { get; set; } // channel if eg. PWM servo or bus ID if bus servo
        public required string Title { get; set; }
        public required int MaxTemperature { get; set; }
        public required int MaxTorque { get; set; }
        public ServoExportTypes ServoExportType { get; set; }
        public required bool GlobalFault { get; set; }
        public required bool WheelMode { get; set; }
    }
}

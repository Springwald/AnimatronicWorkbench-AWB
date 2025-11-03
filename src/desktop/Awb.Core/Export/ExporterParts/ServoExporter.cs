// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Project.Actuators;
using Awb.Core.Project.Servos;

namespace Awb.Core.Export.ExporterParts
{
    public class ServoExporter
    {
        /// <summary>
        /// types of supported servos
        /// </summary>
        public enum ServoTypes
        {
            PWM = 0,
            FeetechSts = 1,
            FeetechScs = 2
        }

        public IEnumerable<string> ExportScsServos(string propertyName, IEnumerable<ScsFeetechServoConfig> servos)
        {
            yield return $"   {propertyName} = new std::vector<StsScsServo>();";
            foreach (var servo in servos)
            {
                var defaultValue = servo.DefaultValue ?? servo.MinValue + (servo.MaxValue - servo.MinValue) / 2;
                var speed = servo.Speed ?? 0;
                var acceleration = 0; // scs servos have no acceleration
                var relaxRangesName = $"{propertyName}_relaxRanges";

                foreach (var relaxRangeLine in ExportRelaxRanges(relaxRangeObject: servo, listName: relaxRangesName))
                    yield return relaxRangeLine;

                // int channel, String const name, int minValue, int maxValue, int defaultValue, int acceleration, int speed, bool globalFault
                yield return $"   {propertyName}->push_back(StsScsServo({servo.Channel}, \"{servo.Title}\", {servo.MinValue}, {servo.MaxValue}, {servo.MaxTemp}, {servo.MaxTorque}, {defaultValue}, {acceleration}, {speed}, {servo.GlobalFault.ToString().ToLower()} ));";
            }
        }

        public IEnumerable<string> ExportStsServos(string propertyName, IEnumerable<StsFeetechServoConfig> servos)
        {
            yield return $"   {propertyName} = new std::vector<StsScsServo>();";
            foreach (var servo in servos)
            {
                var defaultValue = servo.DefaultValue ?? servo.MinValue + (servo.MaxValue - servo.MinValue) / 2;
                var acceleration = servo.Acceleration ?? 0;
                var speed = servo.Speed ?? 0;
                var relaxRangesName = $"{propertyName}_relaxRanges";
                foreach (var relaxRangeLine in ExportRelaxRanges(relaxRangeObject: servo, listName: relaxRangesName))
                    yield return relaxRangeLine;
                // int channel, String const name, int minValue, int maxValue, int defaultValue, int acceleration, int speed, bool globalFault, vector<RelaxRange> relaxRanges
                yield return $"   {propertyName}->push_back(StsScsServo({servo.Channel}, \"{servo.Title}\", {servo.MinValue}, {servo.MaxValue}, {servo.MaxTemp}, {servo.MaxTorque}, {defaultValue}, {acceleration}, {speed}, {servo.GlobalFault.ToString().ToLower()}, {relaxRangesName} ));";
            }
        }

        private static IEnumerable<string> ExportRelaxRanges(ISupportsRelaxRanges relaxRangeObject, string listName)
        {
            var relaxRanges = relaxRangeObject.RelaxRanges;
            yield return $"  auto {listName}= new vector<RelaxRange>();";
            foreach (var range in relaxRanges)
                yield return $"  {listName}->push_back(RelaxRange({range.MinValue}, {range.MaxValue}));";
        }

        public IEnumerable<string> ExportPCS9685PwmServos(IEnumerable<Pca9685PwmServoConfig> pca9685PwmServoConfigs)
        {
            var pca9685PwmServos = pca9685PwmServoConfigs?.OrderBy(s => s.Channel).ToArray() ?? Array.Empty<Pca9685PwmServoConfig>();

            var propertyName = "pca9685PwmServos";
            yield return $"   {propertyName} = new std::vector<Pca9685PwmServo>();";

            foreach (var servo in pca9685PwmServos)
            // int channel, String const name, int minValue, int maxValue, int defaultValue, int acceleration, int speed, bool globalFault
            {
                var defaultValue = servo.DefaultValue ?? servo.MinValue + (servo.MaxValue - servo.MinValue) / 2;
                yield return $"   {propertyName}->push_back(Pca9685PwmServo({servo.I2cAdress}, {servo.Channel}, \"{servo.Title}\", {servo.MinValue}, {servo.MaxValue}, {defaultValue}));";
            }
        }
    }
}

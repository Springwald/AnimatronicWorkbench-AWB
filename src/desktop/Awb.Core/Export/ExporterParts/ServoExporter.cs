// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Project.Actuators;
using Awb.Core.Project.Servos;
using System.Text;

namespace Awb.Core.Export.ExporterParts
{
    public class ServoExporter
    {
        private readonly string _servoListName;

        /// <summary>
        /// types of supported servos
        /// </summary>
        public enum ServoExportTypes
        {
            PWM_SERVO = 0,
            STS_SERVO = 1,
            SCS_SERVO = 2
        }

        public ServoExporter(string servoListName)
        {
            _servoListName = servoListName;
        }

        /// <summary>
        /// Export the given servos into C++ code.
        /// </summary>
        public string ExportServos(IEnumerable<IServoConfig> servoConfigs)
        {
            var result = new StringBuilder();
            result.AppendLine($"   {_servoListName} = new std::vector<Servo*>();");

            int servoIndex = 0;
            foreach (var servoConfig in servoConfigs)
            {
                var servoExport = ExportServo(servoConfig, $"servo_{servoIndex:000}");
                result.AppendLine(servoExport);
                servoIndex++;
            }
            return result.ToString();
        }


        public string ExportServo(IServoConfig servoConfig, string servoVariableName)
        {
            var result = new StringBuilder();

            // export relax ranges for this servo
            var relaxRangesName = $"{servoVariableName}_relaxRanges";
            if (servoConfig is ISupportsRelaxRanges relaxRangeObject)
            {
                result.AppendLine($"std::vector<RelaxRange> *{relaxRangesName} = new std::vector<RelaxRange>();");
                foreach (var relaxRangeLine in ExportRelaxRanges(relaxRangeObject: relaxRangeObject, listName: relaxRangesName))
                    result.AppendLine(relaxRangeLine);
            }
            else
            {
                result.AppendLine($"std::vector<RelaxRange> *{relaxRangesName} = nullptr;");
            }

            // define the variables for the servo parameters
            int defaultValue = -1;
            int acceleration = -1;
            uint i2cAdress = 0;
            int speed = -1;
            uint channel = 0; // channel if eg. PWM servo or bus ID if bus servo
            string title = string.Empty;
            int maxTemperature = -1;
            int maxTorque = -1;
            ServoExportTypes servoExportType;
            bool globalFault = false;

            // define the type of servo 
            switch (servoConfig)
            {
                case Pca9685PwmServoConfig pwmServo:
                    servoExportType = ServoExportTypes.PWM_SERVO;
                    channel = pwmServo.Channel;
                    i2cAdress = pwmServo.I2cAdress;
                    defaultValue = pwmServo.DefaultValue ?? pwmServo.MinValue + (pwmServo.MaxValue - pwmServo.MinValue) / 2;
                    break;
                case StsFeetechServoConfig stsServo:
                    servoExportType = ServoExportTypes.STS_SERVO;
                    channel = stsServo.Channel; 
                    defaultValue = stsServo.DefaultValue ?? stsServo.MinValue + (stsServo.MaxValue - stsServo.MinValue) / 2;
                    acceleration = stsServo.Acceleration ?? 0;
                    speed = stsServo.Speed ?? 0;
                    break;
                case ScsFeetechServoConfig scsServo:
                    servoExportType = ServoExportTypes.SCS_SERVO;
                    channel = scsServo.Channel;
                    defaultValue = scsServo.DefaultValue ?? scsServo.MinValue + (scsServo.MaxValue - scsServo.MinValue) / 2;
                    acceleration = 0; // scs servos have no acceleration
                    speed = scsServo.Speed ?? 0;
                    break;
                default:
                    throw new NotSupportedException($"Exporting servo of type {servoConfig.GetType().FullName} is not supported.");
            }

            result.Append($"    {_servoListName}->push_back(new ServoConfig(");
            result.Append($"ServoConfig::ServoTypes::{nameof(servoExportType)}, "); // the servo type
            result.Append($"\"{servoConfig.Title}\", "); // the servo title
            result.Append($"{channel}, "); // chanel for e.g. PWM servo or bus ID for bus servo
            result.Append($"{i2cAdress}, "); // I2C adress if supported when e.g. PWM servo
            result.Append($"{servoConfig.MinValue}, ");
            result.Append($"{servoConfig.MaxValue}, ");
            result.Append($"{maxTemperature}, "); // max temperature if supported
            result.Append($"{maxTorque}, "); // max torque if supported
            result.Append($"{defaultValue}, ");
            result.Append($"{acceleration}, "); // default acceleration
            result.Append($"{speed}, "); // default speed
            result.Append($"{globalFault}, ");
            result.Append($"{relaxRangesName}"); // relax ranges 
            result.AppendLine(");");
            
            return result.ToString();
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
            //yield return $"  auto {listName}= new vector<RelaxRange>();";
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

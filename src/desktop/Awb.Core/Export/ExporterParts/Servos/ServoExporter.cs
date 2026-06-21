// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2026 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Project;
using Awb.Core.Project.Actuators;
using Awb.Core.Project.Servos;
using System.Text;

namespace Awb.Core.Export.ExporterParts.Servos
{
    public class ServoExporter
    {
        private readonly string _servoListName;

        private int _servoExportIndex = 0;

        public ServoExporter(string servoListName)
        {
            _servoListName = servoListName;
        }

        public static string ServoPointInstanceExport(IDeviceConfig servoConfig, int milliSeconds, int value)
            => $"ServoPoint(\"{servoConfig.Id}\", {milliSeconds}, {value})";

        /// <summary>
        /// Export the given servos into C++ code.
        /// </summary>
        public string ExportServos(IEnumerable<IServoConfig> servoConfigs)
        {
            var result = new StringBuilder();
            //result.AppendLine($"\t\t\t\t{_servoListName} = new std::vector<Servo*>();");
            foreach (var servoConfig in servoConfigs)
            {
                var servoExport = ExportServo(servoConfig, $"servo_{_servoExportIndex:000}");
                result.Append(servoExport);
                _servoExportIndex++;
            }
            return result.ToString();
        }


        public string ExportServo(IServoConfig servoConfig, string servoVariableName)
        {
            var result = new StringBuilder();

            string id = string.Empty;

            if (servoConfig is IDeviceConfig deviceConfig)
            {
                // add comment with servo ID
                id = deviceConfig.Id;
            }
            else
            {
                throw new NotSupportedException($"Exporting servo of type {servoConfig.GetType().FullName} is not supported because it does not implement IDeviceConfig.");
            }

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

            ServoExportModel? exportModel = null;

            switch (servoConfig)
            {
                case Pca9685PwmServoConfig pwmServo:
                    exportModel = new ServoExportModel
                    {
                        Id = pwmServo.Id,
                        ServoExportType = ServoExportModel.ServoExportTypes.PWM_SERVO,
                        Title = pwmServo.Title,
                        DefaultValue = pwmServo.DefaultValue ?? pwmServo.MinValue + (pwmServo.MaxValue - pwmServo.MinValue) / 2,
                        I2cAdress = pwmServo.I2cAdress,
                        Channel = pwmServo.Channel,
                        Acceleration = 0, // PWM servos have no acceleration
                        Speed = 0, // PWM servos have no speed
                        MaxTemperature = -1, // PWM servos have no max temperature
                        MaxTorque = -1, // PWM servos have no max torque
                        GlobalFault = false, // PWM servos have no global fault,
                        WheelMode = false // PWM servos have no wheel mode
                    };
                    break;
                case StsFeetechServoConfig stsServo:
                    exportModel = new ServoExportModel
                    {
                        Id = stsServo.Id,
                        ServoExportType = ServoExportModel.ServoExportTypes.STS_SERVO,
                        Title = stsServo.Title,
                        DefaultValue = stsServo.DefaultValue ?? stsServo.MinValue + (stsServo.MaxValue - stsServo.MinValue) / 2,
                        I2cAdress = 0, // STS servos have no I2C address
                        Channel = stsServo.Channel,
                        Acceleration = stsServo.Acceleration ?? 0,
                        Speed = stsServo.Speed ?? 0,
                        MaxTemperature = (int)stsServo.MaxTemp,
                        MaxTorque = (int)stsServo.MaxTorque,
                        GlobalFault = stsServo.GlobalFault,
                        WheelMode = stsServo.WheelMode
                    };
                    break;
                case ScsFeetechServoConfig scsServo:
                    exportModel = new ServoExportModel
                    {
                        Id = scsServo.Id,
                        ServoExportType = ServoExportModel.ServoExportTypes.SCS_SERVO,
                        Title = scsServo.Title,
                        DefaultValue = scsServo.DefaultValue ?? scsServo.MinValue + (scsServo.MaxValue - scsServo.MinValue) / 2,
                        I2cAdress = 0, // SCS servos have no I2C address
                        Channel = scsServo.Channel,
                        Acceleration = 0, // SCS servos have no acceleration
                        Speed = scsServo.Speed ?? 0,
                        MaxTemperature = (int)scsServo.MaxTemp,
                        MaxTorque = (int)scsServo.MaxTorque,
                        GlobalFault = scsServo.GlobalFault,
                        WheelMode = false // SCS servos have no wheel mode
                    };
                    break;
                default:
                    throw new NotSupportedException($"Exporting servo of type {servoConfig.GetType().FullName} is not supported.");
            }
            if (exportModel == null)
                throw new InvalidOperationException($"Exporting servo of type {servoConfig.GetType().FullName} failed because the export model could not be created.");


            result.Append($"\t\t{_servoListName}->addServo(Servo(\"{id}\", new ServoConfig(");
            result.Append($"ServoConfig::ServoTypes::{exportModel.ServoExportType.ToString()}, "); // the servo type
            result.Append($"\"{servoConfig.Title}\", "); // the servo title
            result.Append($"{exportModel.Channel}, "); // chanel for e.g. PWM servo or bus ID for bus servo
            result.Append($"{exportModel.I2cAdress}, "); // I2C adress if supported when e.g. PWM servo
            result.Append($"{servoConfig.MinValue}, "); // min value for this servo
            result.Append($"{servoConfig.MaxValue}, "); // max value for this servo
            result.Append($"{exportModel.MaxTemperature}, "); // max temperature if supported
            result.Append($"{exportModel.MaxTorque}, "); // max torque if supported
            result.Append($"{exportModel.DefaultValue}, "); // default value for this servo
            result.Append($"{exportModel.Acceleration}, "); // default acceleration
            result.Append($"{exportModel.Speed}, "); // default speed
            result.Append($"{exportModel.GlobalFault.ToString().ToLower()}, "); // global fault if supported
            result.Append($"{exportModel.WheelMode.ToString().ToLower()}, "); // wheel mode if supported
            result.Append($"{relaxRangesName}"); // relax ranges 
            result.AppendLine(")));");

            return result.ToString();
        }


        private static IEnumerable<string> ExportRelaxRanges(ISupportsRelaxRanges relaxRangeObject, string listName)
        {
            var relaxRanges = relaxRangeObject.RelaxRanges;
            //yield return $"  auto {listName}= new vector<RelaxRange>();";
            foreach (var range in relaxRanges)
                yield return $"\t\t{listName}->push_back(RelaxRange({range.MinValue}, {range.MaxValue}));";
        }

        public IEnumerable<string> ExportPCS9685PwmServos(IEnumerable<Pca9685PwmServoConfig> pca9685PwmServoConfigs)
        {
            var pca9685PwmServos = pca9685PwmServoConfigs?.OrderBy(s => s.Channel).ToArray() ?? Array.Empty<Pca9685PwmServoConfig>();

            var propertyName = "pca9685PwmServos";
            yield return $"\t\t{propertyName} = new std::vector<Pca9685PwmServo>();";

            foreach (var servo in pca9685PwmServos)
            // int channel, String const name, int minValue, int maxValue, int defaultValue, int acceleration, int speed, bool globalFault
            {
                var defaultValue = servo.DefaultValue ?? servo.MinValue + (servo.MaxValue - servo.MinValue) / 2;
                yield return $"\t\t{propertyName}->push_back(Pca9685PwmServo({servo.I2cAdress}, {servo.Channel}, \"{servo.Title}\", {servo.MinValue}, {servo.MaxValue}, {defaultValue}));";
            }
        }
    }
}

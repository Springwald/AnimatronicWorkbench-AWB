// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License


using Awb.Core.Export.ExporterParts.ExportData;
using Awb.Core.Project.Servos;
using Awb.Core.Project.Various;
using Awb.Core.Timelines;
using System.Text;

namespace Awb.Core.Export.ExporterParts
{
    internal class ProjectDataExporter : ExporterPartAbstract
    {
        private readonly ProjectExportData _projectData;
        private readonly string _targetFolder;

        public ProjectDataExporter(ProjectExportData projectData, string targetFolder)
        {
            _projectData = projectData;
            _targetFolder = targetFolder;
        }

        public override async Task<IExporter.ExportResult> ExportAsync()
        {
            // check the target folder
            if (!Directory.Exists(_targetFolder)) return new IExporter.ExportResult { ErrorMessage = $"Target folder '{_targetFolder}' not found" };

            // export project data
            var result = await WriteProjectDataHAsync(_targetFolder);
            if (result.Success == false) return result;

            // export hardware.h
            result = await WriteHardwareHAsync(_targetFolder);
            if (result.Success == false) return result;

            return result;
        }

        private async Task<IExporter.ExportResult> WriteHardwareHAsync(string folder)
        {
            var content = new StringBuilder();

            content.AppendLine("#ifndef hardware_config_h");
            content.AppendLine("#define hardware_config_h");
            content.AppendLine();

            if (_projectData.Esp32ClientHardwareConfig.DebuggingIoPin != null)
            {
                content.AppendLine("/* Debugging settings */");
                content.AppendLine($"#define DEBUGGING_IO_PIN {_projectData.Esp32ClientHardwareConfig.DebuggingIoPin} // the GPIO pin to use for debugging");
                content.AppendLine($"#define DEBUGGING_IO_PIN_ACTIVE {_projectData.Esp32ClientHardwareConfig.DebuggingIoPinActiveState} // if the debugging pin is active low, set this to true");
                content.AppendLine();
            }

            // Display settings
            content.AppendLine("/* Display settings */");
            content.AppendLine("");

            if (_projectData.Esp32ClientHardwareConfig.Display_M5Stack)
            {
                content.AppendLine("// -- M5 Stack Displays --");
                content.AppendLine("#define DISPLAY_M5STACK // if you use a M5Stack, uncomment this line. Uses GPIO 14+18 for display communication");
            }
            if (_projectData.Esp32ClientHardwareConfig.Display_Ssd1306)
            {
                content.AppendLine("// -- SSD1306 Displays  --");
                content.AppendLine("// e.g. for Waveshare Servo Driver with ESP32: 128x32, 0x02, 0x3C");
                content.AppendLine("#define DISPLAY_SSD1306");
                content.AppendLine($"#define DISPLAY_SSD1306_I2C_ADDRESS {_projectData.Esp32ClientHardwareConfig.Ssd1306I2cAddress} // 0x3C or 0x3D, See datasheet for Address; 0x3D for 128x64, 0x3C for 128x32");
                content.AppendLine($"#define DISPLAY_SSD1306_WIDTH {_projectData.Esp32ClientHardwareConfig.Ssd1306ScreenWidth}");
                content.AppendLine($"#define DISPLAY_SSD1306_HEIGHT {_projectData.Esp32ClientHardwareConfig.Ssd1306ScreenHeight}");
                content.AppendLine($"#define DISPLAY_SSD1306_COM_PINS {_projectData.Esp32ClientHardwareConfig.Ssd1306ComPins} // 0x02, 0x12, 0x22 or 0x32");
            }
            content.AppendLine();

            // Servos
            if (_projectData.Esp32ClientHardwareConfig.UseStsServos)
            {
                content.AppendLine("/* STS serial servo settings */");
                content.AppendLine($"#define USE_STS_SERVO");
                content.AppendLine($"#define STS_SERVO_RXD {_projectData.Esp32ClientHardwareConfig.StsRXPin}");
                content.AppendLine($"#define STS_SERVO_TXD {_projectData.Esp32ClientHardwareConfig.StsTXPin}");
                content.AppendLine();
            }
            if (_projectData.Esp32ClientHardwareConfig.UseScsServos)
            {
                content.AppendLine("/* SCS serial servo settings */");
                content.AppendLine($"#define USE_SCS_SERVO");
                content.AppendLine($"#define SCS_SERVO_RXD {_projectData.Esp32ClientHardwareConfig.ScsRXPin}");
                content.AppendLine($"#define SCS_SERVO_TXD {_projectData.Esp32ClientHardwareConfig.ScsTXPin}");
                content.AppendLine();
            }
            if (_projectData.Pca9685PwmServoConfigs.Any())
            {
                content.AppendLine("/* PCA9685 PWM servo settings */");
                content.AppendLine("#define USE_PCA9685_PWM_SERVO");
                content.AppendLine("#define PCA9685_OSC_FREQUENCY 25000000");
                content.AppendLine();
            }


            // DAC speaker settings
            if (false)
            {
                content.AppendLine("/* DAC speaker */");
                content.AppendLine("// #define USE_DAC_SPEAKER");
            }

            // Status neopixel
            if (_projectData.Esp32ClientHardwareConfig.UseNeoPixel)
            {
                content.AppendLine("/* Neopixel RGB LEDs */");
                content.AppendLine("#define USE_NEOPIXEL");
                content.AppendLine($"#define NEOPIXEL_GPIO {_projectData.Esp32ClientHardwareConfig.NeoPixelPin}");
                content.AppendLine($"#define NEOPIXEL_COUNT {_projectData.Esp32ClientHardwareConfig.NeoPixelCount}");
                content.AppendLine();
            }

            content.AppendLine();
            content.AppendLine("#endif");

            await File.WriteAllTextAsync(Path.Combine(folder, "HardwareConfig.h"), content.ToString());
            return IExporter.ExportResult.SuccessResult;
        }

        private async Task<IExporter.ExportResult> WriteProjectDataHAsync(string folder)
        {
            var content = new StringBuilder();

            var includes = """
                #include <Arduino.h>
                #include <String.h>
                #include "../ProjectData/Timeline.h"
                #include "../ProjectData/TimelineState.h"
                #include "../ProjectData/TimelineState.h"
                #include "../ProjectData/TimelineStateReference.h"

                #include "../ProjectData/Servos/StsServoPoint.h"
                #include "../ProjectData/Servos/Pca9685PwmServoPoint.h"
                #include "../ProjectData/Servos/StsScsServo.h"
                #include "../ProjectData/Servos/Pca9685PwmServo.h"

                #include "../ProjectData/Mp3Player/Mp3PlayerYX5300Serial.h"
                #include "../ProjectData/Mp3Player/Mp3PlayerDfPlayerMiniSerial.h"
                #include "../ProjectData/Mp3Player/Mp3PlayerYX5300Point.h"
                #include "../ProjectData/Mp3Player/Mp3PlayerDfPlayerMiniPoint.h"
                """;

            content.AppendLine(GetHeader(className: "ProjectData", includes: includes));

            content.AppendLine("using TCallBackErrorOccured = std::function<void(String)>;");
            content.AppendLine();
            content.AppendLine("public:");
            content.AppendLine($"   const char *ProjectName = \"{_projectData.ProjectName}\";");
            content.AppendLine();

            content.AppendLine($"   const int returnToAutoModeAfterMinutes  = {_projectData.Esp32ClientHardwareConfig.AutoPlayAfter ?? -1} ;");
            content.AppendLine();


            ExportKnownNamesAsConsts(content);

            content.AppendLine();
            content.AppendLine($"\tstd::vector<StsScsServo> *scsServos;");
            content.AppendLine($"\tstd::vector<StsScsServo> *stsServos;");
            content.AppendLine($"\tstd::vector<Pca9685PwmServo> *pca9685PwmServos;");
            content.AppendLine($"\tstd::vector<TimelineState>* timelineStates;");
            content.AppendLine($"\tstd::vector<Timeline>* timelines;");
            content.AppendLine($"\tstd::vector<Mp3PlayerYX5300Serial> *mp3PlayersYX5300;");
            content.AppendLine($"\tstd::vector<Mp3PlayerDfPlayerMiniSerial> *mp3PlayersDfPlayerMini;");

            content.AppendLine();

            ExportInputs(inputConfigs: _projectData.InputConfigs, content);
            content.AppendLine();

            content.AppendLine("ProjectData(TCallBackErrorOccured errorOccured)");
            content.AppendLine("{");
            content.AppendLine();
            ExportScsServos(propertyName: "scsServos", servos: _projectData.ScsServoConfigs, content);
            ExportStsServos(propertyName: "stsServos", servos: _projectData.StsServoConfigs, content);
            ExportPCS9685PwmServos(_projectData.Pca9685PwmServoConfigs, content);
            ExportMp3PlayerYX5300Informations(_projectData.Mp3PlayerYX5300Configs, content);
            ExportMp3PlayerDfPlayerMiniInformations(_projectData.Mp3PlayerDfPlayerMiniConfigs, content);
            ExportTimelineStates(_projectData.TimelineStates, content);
            content.AppendLine();
            content.AppendLine("\taddTimelines();");
            content.AppendLine();
            content.AppendLine("}");

            // Add timelines

            content.AppendLine();
            content.AppendLine("void addTimelines() {");

            content.AppendLine("\ttimelines = new std::vector<Timeline>();");
            content.AppendLine();

            var errMsg = ExportTimelineData(_projectData, content);
            if (errMsg != null)
                return new IExporter.ExportResult { ErrorMessage = errMsg };

            content.AppendLine();
            content.AppendLine("}");

            content.AppendLine(GetFooter("ProjectData"));

            await File.WriteAllTextAsync(Path.Combine(folder, "ProjectData.h"), content.ToString());
            return IExporter.ExportResult.SuccessResult;
        }

        private void ExportKnownNamesAsConsts(StringBuilder content)
        {
            content.AppendLine($"   /* Names as const to prevent magic strings in custom code: */");
            content.AppendLine();
            // timeline names
            foreach (var timeline in _projectData.TimelineData)
                content.AppendLine($"   static inline const std::string TimelineName_{CleanUpName(timeline.Title)} = \"{timeline.Title}\";");
            // mp3 player names
            foreach (var mp3Player in _projectData.Mp3PlayerYX5300Configs)
                content.AppendLine($"   static inline const std::string Mp3PlayerName_{CleanUpName(mp3Player.Title)} = \"{mp3Player.Title}\";");
            foreach (var mp3Player in _projectData.Mp3PlayerDfPlayerMiniConfigs)
                content.AppendLine($"   static inline const std::string Mp3PlayerName_{CleanUpName(mp3Player.Title)} = \"{mp3Player.Title}\";");
            // servo names
            foreach (var servo in _projectData.ScsServoConfigs)
                content.AppendLine($"   static inline const std::string ScsServoName_{CleanUpName(servo.Title)} = \"{servo.Title}\";");
            foreach (var servo in _projectData.StsServoConfigs)
                content.AppendLine($"   static inline const std::string StsServoName_{CleanUpName(servo.Title)} = \"{servo.Title}\";");
            foreach (var servo in _projectData.Pca9685PwmServoConfigs)
                content.AppendLine($"   static inline const std::string Pca9685PwmServoName_{CleanUpName(servo.Title)} = \"{servo.Title}\";");
            content.AppendLine();
        }

        private static string CleanUpName(string name)
        {
            var charsToReplace = new[] { " ", "-", "ä", "ö", "ü", "ß", "Ä", "Ö", "Ü", "/", @"\" };
            foreach (var c in charsToReplace)
                name = name.Replace(c, "");
            return name;
        }

        private static void ExportInputs(IEnumerable<InputConfig> inputConfigs, StringBuilder result)
        {
            // export the inputs
            var exportInputs = inputConfigs ?? Array.Empty<InputConfig>();
            result.AppendLine($"\tint inputIds[{exportInputs.Count()}] = {{{string.Join(", ", exportInputs.Select(i => i.Id.ToString()))}}};");
            result.AppendLine($"\tString inputNames[{exportInputs.Count()}] = {{{string.Join(", ", exportInputs.Select(s => $"\"{s.Title}\""))}}};");
            result.AppendLine($"\tuint8_t  inputIoPins[{exportInputs.Count()}] = {{{string.Join(", ", exportInputs.Select(s => s.IoPin.ToString()))}}};");
            result.AppendLine($"\tint inputCount = {exportInputs.Count()};");
            result.AppendLine();
        }

        private static void ExportTimelineStates(IEnumerable<TimelineState> timelineStates, StringBuilder result)
        {
            var exportStates = timelineStates?.Where(s => s.Export).ToArray() ?? Array.Empty<TimelineState>();
            result.AppendLine($"\ttimelineStates = new std::vector<TimelineState>();");
            foreach (var state in exportStates)
            {
                // add line in using this format:  timelineStates->push_back(TimelineState(1, String("InBag"), true, new std::vector<int>({1}), new std::vector<int>({0}))); 
                result.AppendLine($"\ttimelineStates->push_back(TimelineState({state.Id}, String(\"{state.Title}\"), {state.AutoPlay.ToString().ToLower()}, new std::vector<int>({{ {string.Join(", ", state.PositiveInputs)} }}), new std::vector<int>({{ {string.Join(", ", state.NegativeInputs)} }})));");
            }
        }

        private static void ExportScsServos(string propertyName, IEnumerable<ScsFeetechServoConfig> servos, StringBuilder result)
        {
            result.AppendLine($"   {propertyName} = new std::vector<StsScsServo>();");
            foreach (var servo in servos)
            {
                var defaultValue = servo.DefaultValue ?? servo.MinValue + (servo.MaxValue - servo.MinValue) / 2;
                var speed = servo.Speed ?? 0;
                var acceleration = 0; // scs servos have no acceleration
                // int channel, String const name, int minValue, int maxValue, int defaultValue, int acceleration, int speed, bool globalFault
                result.AppendLine($"   {propertyName}->push_back(StsScsServo({servo.Channel}, \"{servo.Title}\", {servo.MinValue}, {servo.MaxValue}, {servo.MaxTemp}, {servo.MaxTorque}, {defaultValue}, {acceleration}, {speed}, {servo.GlobalFault.ToString().ToLower()} ));");
            }
            result.AppendLine();
        }

        private static void ExportStsServos(string propertyName, IEnumerable<StsFeetechServoConfig> servos, StringBuilder result)
        {
            result.AppendLine($"   {propertyName} = new std::vector<StsScsServo>();");
            foreach (var servo in servos)
            {
                var defaultValue = servo.DefaultValue ?? servo.MinValue + (servo.MaxValue - servo.MinValue) / 2;
                var acceleration = servo.Acceleration ?? 0;
                var speed = servo.Speed ?? 0;
                // int channel, String const name, int minValue, int maxValue, int defaultValue, int acceleration, int speed, bool globalFault
                result.AppendLine($"   {propertyName}->push_back(StsScsServo({servo.Channel}, \"{servo.Title}\", {servo.MinValue}, {servo.MaxValue}, {servo.MaxTemp}, {servo.MaxTorque}, {defaultValue}, {acceleration}, {speed}, {servo.GlobalFault.ToString().ToLower()} ));");
            }
            result.AppendLine();
        }

        private static void ExportPCS9685PwmServos(IEnumerable<Pca9685PwmServoConfig> pca9685PwmServoConfigs, StringBuilder result)
        {
            var pca9685PwmServos = pca9685PwmServoConfigs?.OrderBy(s => s.Channel).ToArray() ?? Array.Empty<Pca9685PwmServoConfig>();

            var propertyName = "pca9685PwmServos";
            result.AppendLine($"   {propertyName} = new std::vector<Pca9685PwmServo>();");

            foreach (var servo in pca9685PwmServos)
                // int channel, String const name, int minValue, int maxValue, int defaultValue, int acceleration, int speed, bool globalFault
                result.AppendLine($"   {propertyName}->push_back(Pca9685PwmServo({servo.I2cAdress}, {servo.Channel}, \"{servo.Title}\", {servo.MinValue}, {servo.MaxValue}, {servo.DefaultValue}));");

            result.AppendLine();
        }


        private static void ExportMp3PlayerYX5300Informations(IEnumerable<Mp3PlayerYX5300Config>? mp3PlayerYX5300Configs, StringBuilder result)
        {
            var players = mp3PlayerYX5300Configs ?? Array.Empty<Mp3PlayerYX5300Config>();
            // add  mp3 players using the constructor:  Mp3PlayerYX5300Serial(int rxPin, int txPin, String name) 
            result.AppendLine($"\tmp3PlayersYX5300 = new std::vector<Mp3PlayerYX5300Serial>();");
            foreach (var player in players)
                result.AppendLine($"\tmp3PlayersYX5300->push_back(Mp3PlayerYX5300Serial({player.RxPin}, {player.TxPin}, \"{player.Title}\", \"{player.Id}\"));");

            result.AppendLine();
        }

        private static void ExportMp3PlayerDfPlayerMiniInformations(IEnumerable<Mp3PlayerDfPlayerMiniConfig>? mp3PlayerDfPlayerMiniConfigs, StringBuilder result)
        {
            var players = mp3PlayerDfPlayerMiniConfigs ?? Array.Empty<Mp3PlayerDfPlayerMiniConfig>();
            // add  mp3 players using the constructor:  Mp3PlayerDfPlayerMiniConfig(int rxPin, int txPin, int volume, String name) 
            result.AppendLine($"\tmp3PlayersDfPlayerMini = new std::vector<Mp3PlayerDfPlayerMiniSerial>();");
            foreach (var player in players)
                result.AppendLine($"\tmp3PlayersDfPlayerMini->push_back(Mp3PlayerDfPlayerMiniSerial({player.RxPin}, {player.TxPin}, {player.Volume}, \"{player.Title}\", \"{player.Id}\", errorOccured));");

            result.AppendLine();
        }

        /// <returns>null=ok, else the error message</returns>
        private static string? ExportTimelineData(ProjectExportData projectData, StringBuilder result)
        {
            int timelineNo = 1;
            var timelines = projectData.TimelineData.OrderBy(t => t.TimelineStateId).ThenBy(t => t.Title).ToArray();
            foreach (var timeline in timelines)
            {
                var state = projectData.TimelineStates?.SingleOrDefault(x => x.Id == timeline.TimelineStateId);

                if (state == null)
                    return $"Timeline '{timeline.Title}' uses an undefined timelineStateId '{timeline.TimelineStateId}";

                if (state.Export == false) continue;

                result.AppendLine($"\t\tauto *stsServoPoints{timelineNo} = new std::vector<StsServoPoint>();");
                result.AppendLine($"\t\tauto *scsServoPoints{timelineNo} = new std::vector<StsServoPoint>();");
                result.AppendLine($"\t\tauto *pca9685PwmServoPoints{timelineNo} = new std::vector<Pca9685PwmServoPoint>();");
                result.AppendLine($"\t\tauto *mp3PlayerYX5300Points{timelineNo} = new std::vector<Mp3PlayerYX5300Point>();");
                result.AppendLine($"\t\tauto *mp3PlayerDfPlayerMiniPoints{timelineNo} = new std::vector<Mp3PlayerDfPlayerMiniPoint>();");

                // Export Servo-Points
                foreach (var servoPoint in timeline.Points.OfType<ServoPoint>().OrderBy(p => p.TimeMs))
                {
                    // find STS servo
                    var stsServo = projectData.StsServoConfigs?.SingleOrDefault(s => s.Id == servoPoint.ServoId);
                    if (stsServo != null)
                    {
                        var value = (int)(stsServo.MinValue + servoPoint.ValuePercent * (stsServo.MaxValue - stsServo.MinValue) / 100.0);
                        result.AppendLine($"\t\tstsServoPoints{timelineNo}->push_back(StsServoPoint({stsServo.Channel},{servoPoint.TimeMs},{value}));");
                        continue;
                    }

                    // find SCS servo
                    var scsServo = projectData.ScsServoConfigs?.SingleOrDefault(s => s.Id == servoPoint.ServoId);
                    if (scsServo != null)
                    {
                        var value = (int)(scsServo.MinValue + servoPoint.ValuePercent * (scsServo.MaxValue - scsServo.MinValue) / 100.0);
                        result.AppendLine($"\t\tscsServoPoints{timelineNo}->push_back(StsServoPoint({scsServo.Channel},{servoPoint.TimeMs},{value}));");
                        continue;
                    }

                    // find pwm servo
                    var pwmServo = projectData.Pca9685PwmServoConfigs?.SingleOrDefault(s => s.Id == servoPoint.ServoId);
                    if (pwmServo != null)
                    {
                        var value = (int)(pwmServo.MinValue + servoPoint.ValuePercent * (pwmServo.MaxValue - pwmServo.MinValue) / 100.0);
                        result.AppendLine($"\t\tpca9685PwmServoPoints{timelineNo}->push_back(Pca9685PwmServoPoint({pwmServo.I2cAdress},{pwmServo.Channel},{servoPoint.TimeMs},{value}));");
                        continue;
                    }

                    // todo: find other servos

                    // servo id not sound
                    return $"Servo id '{servoPoint.ServoId}' not found in project config!";
                }

                // Export YX5300 Sound-Points
                foreach (var soundPoint in timeline.Points.OfType<SoundPoint>().OrderBy(p => p.TimeMs))
                {
                    if (projectData.Mp3PlayerYX5300Configs != null)
                    {
                        // find Mp3PlayerYX5300 soundplayer
                        var soundPlayerIndex = -1;
                        for (int i = 0; i < projectData.Mp3PlayerYX5300Configs?.Count(); i++)
                        {
                            if (projectData.Mp3PlayerYX5300Configs.ElementAt(i).Id == soundPoint.SoundPlayerId)
                            {
                                soundPlayerIndex = i;
                                break;
                            }
                        }
                        if (soundPlayerIndex != -1)
                        {
                            var soundPlayer = projectData.Mp3PlayerYX5300Configs!.ElementAt(soundPlayerIndex);
                            if (soundPlayer != null)
                            {
                                result.AppendLine($"\t\tmp3PlayerYX5300Points{timelineNo}->push_back(Mp3PlayerYX5300Point({soundPoint.SoundId}, {soundPlayerIndex}, {soundPoint.TimeMs}));");
                                continue;
                            }
                        }
                    }


                    if (projectData.Mp3PlayerDfPlayerMiniConfigs != null)
                    {
                        // find Mp3PlayerDfPlayerMini soundplayer
                        var soundPlayerIndex = -1;
                        for (int i = 0; i < projectData.Mp3PlayerDfPlayerMiniConfigs?.Count(); i++)
                        {
                            if (projectData.Mp3PlayerDfPlayerMiniConfigs.ElementAt(i).Id == soundPoint.SoundPlayerId)
                            {
                                soundPlayerIndex = i;
                                break;
                            }
                        }
                        if (soundPlayerIndex != -1)
                        {
                            var soundPlayer = projectData.Mp3PlayerDfPlayerMiniConfigs!.ElementAt(soundPlayerIndex);
                            if (soundPlayer != null)
                            {
                                result.AppendLine($"\t\tmp3PlayerDfPlayerMiniPoints{timelineNo}->push_back(Mp3PlayerDfPlayerMiniPoint({soundPoint.SoundId}, {soundPlayerIndex}, {soundPoint.TimeMs}));");
                                continue;
                            }
                        }
                    }

                    // todo: find other sound player

                    // servo id not sound
                    return $"Soundplayer id '{soundPoint.SoundPlayerId}' not found in project config!";
                }


                result.AppendLine($"\t\tauto state{timelineNo} = new TimelineStateReference({state.Id}, String(\"{state.Title}\"));");
                result.AppendLine($"\t\tTimeline *timeline{timelineNo} = new Timeline(state{timelineNo}, {timeline.NextTimelineStateOnceId ?? -1}, String(\"{timeline.Title}\"), stsServoPoints{timelineNo}, scsServoPoints{timelineNo}, pca9685PwmServoPoints{timelineNo}, mp3PlayerYX5300Points{timelineNo}, mp3PlayerDfPlayerMiniPoints{timelineNo});");
                result.AppendLine($"\t\ttimelines->push_back(*timeline{timelineNo});");

                result.AppendLine();

                timelineNo++;
            }
            return null;
        }

    }
}

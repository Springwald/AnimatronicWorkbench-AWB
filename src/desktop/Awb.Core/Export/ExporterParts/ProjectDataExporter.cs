// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License


using Awb.Core.Export.ExporterParts.ExportData;
using Awb.Core.Project.Various;
using Awb.Core.Timelines;
using Awb.Core.Timelines.Sounds;
using System.Text;
using TagLib.IFD.Tags;

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

            content.AppendLine($"#define AwbClientId {_projectData.Esp32ClientHardwareConfig.ClientId} // If you use more than one AWB-client, you have to enter different IDs per client here");
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
#pragma warning disable CS0162 // Unreachable code detected
                content.AppendLine("/* DAC speaker */");
                content.AppendLine("// #define USE_DAC_SPEAKER");
#pragma warning restore CS0162 // Unreachable code detected
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
            const string className = "ProjectData";
            const string servoListName = "servos";
            var servoExporter = new ServoExporter(servoListName: servoListName);
            var timelineExporter = new TimelineDataExporter();
            var timelineExportData = timelineExporter.ExportTimelineData(_projectData);

            if (timelineExportData.ErrorMessage != null)
                return new IExporter.ExportResult { ErrorMessage = timelineExportData.ErrorMessage };

            content.AppendLine(
                $$"""

                {{Get_H_Header(className: className)}}

                #include <Arduino.h>
                #include <String.h>
                #include "../ProjectData/Timeline.h"
                #include "../ProjectData/TimelineState.h"
                #include "../ProjectData/TimelineState.h"
                #include "../ProjectData/TimelineStateReference.h"

                #include "../ProjectData/Servos/ServoPoint.h"
                #include "../ProjectData/Servos/Servo.h"
                #include <ProjectData/Servos/Servos.h>

                #include "../ProjectData/Mp3Player/Mp3PlayerYX5300Serial.h"
                #include "../ProjectData/Mp3Player/Mp3PlayerDfPlayerMiniSerial.h"
                #include "../ProjectData/Mp3Player/Mp3PlayerYX5300Point.h"
                #include "../ProjectData/Mp3Player/Mp3PlayerDfPlayerMiniPoint.h"

                {{GetHeader(className: className)}}

                using TCallBackErrorOccured = std::function<void(String)>;

                public:
                    const char *ProjectName = "{{_projectData.ProjectName}}";
                    const int returnToAutoModeAfterMinutes  = {{_projectData.Esp32ClientHardwareConfig.AutoPlayAfter ?? -1}};

                {{ExportKnownNamesAsConsts()}}

                Servos *{{servoListName}};
                std::vector<TimelineState>* timelineStates;
                std::vector<Timeline>* timelines;
                std::vector<Mp3PlayerYX5300Serial> *mp3PlayersYX5300;
                std::vector<Mp3PlayerDfPlayerMiniSerial> *mp3PlayersDfPlayerMini;

                {{ExportInputs(inputConfigs: _projectData.InputConfigs)}}

                ProjectData(TCallBackErrorOccured errorOccured)
                {
                    // the servos
                    {{servoListName}} = new Servos();
                    {{servoExporter.ExportServos(servoConfigs: _projectData.ScsServoConfigs)}}
                    {{servoExporter.ExportServos(servoConfigs: _projectData.StsServoConfigs)}}
                    {{servoExporter.ExportServos(servoConfigs: _projectData.Pca9685PwmServoConfigs)}}
                
                    // sound player
                    {{ExportMp3PlayerYX5300Informations(_projectData.Mp3PlayerYX5300Configs)}}
                    {{ExportMp3PlayerDfPlayerMiniInformations(_projectData.Mp3PlayerDfPlayerMiniConfigs)}}

                    // timelines states
                    {{ExportTimelineStates(_projectData.TimelineStates)}}    

                    addTimelines();
                }

                // timelines
                void addTimelines() 
                {
                    timelines = new std::vector<Timeline>();
                    {{timelineExportData.Content}}
                }

                {{GetFooter("ProjectData")}}
            
            """
            );

            await File.WriteAllTextAsync(Path.Combine(folder, "ProjectData.h"), content.ToString());
            return IExporter.ExportResult.SuccessResult;
        }

        private string ExportKnownNamesAsConsts()
        {
            StringBuilder result = new StringBuilder();
            result.AppendLine($"   /* Names as const to prevent magic strings in custom code: */");
            result.AppendLine();

            var constString = (string praefix, string s) => $"   const String {praefix}{CleanUpName(s)} =\"{s}\";";

            // timeline names
            foreach (var timeline in _projectData.TimelineData)
                result.AppendLine(constString("TimelineName_", timeline.Title));

            // mp3 player names
            foreach (var mp3Player in _projectData.Mp3PlayerYX5300Configs)
                result.AppendLine(constString("Mp3PlayerName_", mp3Player.Title));
            foreach (var mp3Player in _projectData.Mp3PlayerDfPlayerMiniConfigs)
                result.AppendLine(constString("Mp3PlayerName_", mp3Player.Title));

            // servo names
            foreach (var servo in _projectData.ScsServoConfigs)
                result.AppendLine(constString("ScsServoName_", servo.Title));
            foreach (var servo in _projectData.StsServoConfigs)
                result.AppendLine(constString("StsServoName_", servo.Title));
            foreach (var servo in _projectData.Pca9685PwmServoConfigs)
                result.AppendLine(constString("Pca9685PwmServoName_", servo.Title));
            result.AppendLine();

            return result.ToString();
        }

        private static string CleanUpName(string name)
        {
            var charsToReplace = new[] { " ", "-", "ä", "ö", "ü", "ß", "Ä", "Ö", "Ü", "/", @"\" };
            foreach (var c in charsToReplace)
                name = name.Replace(c, "");
            return name;
        }

        private static string ExportInputs(IEnumerable<InputConfig> inputConfigs)
        {
            StringBuilder result = new StringBuilder();

            // export the inputs
            var exportInputs = inputConfigs ?? Array.Empty<InputConfig>();
            result.AppendLine($"\tint inputIds[{exportInputs.Count()}] = {{{string.Join(", ", exportInputs.Select(i => i.Id.ToString()))}}};");
            result.AppendLine($"\tString inputNames[{exportInputs.Count()}] = {{{string.Join(", ", exportInputs.Select(s => $"\"{s.Title}\""))}}};");
            result.AppendLine($"\tuint8_t  inputIoPins[{exportInputs.Count()}] = {{{string.Join(", ", exportInputs.Select(s => s.IoPin.ToString()))}}};");
            result.AppendLine($"\tint inputCount = {exportInputs.Count()};");
            result.AppendLine();

            return result.ToString();
        }

        private static string ExportTimelineStates(IEnumerable<TimelineState> timelineStates)
        {
             StringBuilder result = new StringBuilder();
            var exportStates = timelineStates?.Where(s => s.Export).ToArray() ?? Array.Empty<TimelineState>();
            result.AppendLine($"\ttimelineStates = new std::vector<TimelineState>();");
            foreach (var state in exportStates)
            {
                // add line in using this format:  timelineStates->push_back(TimelineState(1, String("InBag"), true, new std::vector<int>({1}), new std::vector<int>({0}))); 
                result.AppendLine($"\ttimelineStates->push_back(TimelineState({state.Id}, String(\"{state.Title}\"), {state.AutoPlay.ToString().ToLower()}, new std::vector<int>({{ {string.Join(", ", state.PositiveInputs)} }}), new std::vector<int>({{ {string.Join(", ", state.NegativeInputs)} }})));");
            }
            result.AppendLine();
            return result.ToString();
        }

        private static string ExportMp3PlayerYX5300Informations(IEnumerable<Mp3PlayerYX5300Config>? mp3PlayerYX5300Configs)
        {
            StringBuilder result = new StringBuilder();
            var players = mp3PlayerYX5300Configs ?? Array.Empty<Mp3PlayerYX5300Config>();
            // add  mp3 players using the constructor:  Mp3PlayerYX5300Serial(int rxPin, int txPin, String name) 
            result.AppendLine($"\tmp3PlayersYX5300 = new std::vector<Mp3PlayerYX5300Serial>();");
            foreach (var player in players)
                result.AppendLine($"\tmp3PlayersYX5300->push_back(Mp3PlayerYX5300Serial({player.RxPin}, {player.TxPin}, \"{player.Title}\", \"{player.Id}\",errorOccured));");

            result.AppendLine();
            return result.ToString();
        }

        private static string ExportMp3PlayerDfPlayerMiniInformations(IEnumerable<Mp3PlayerDfPlayerMiniConfig>? mp3PlayerDfPlayerMiniConfigs)
        {
            StringBuilder result = new StringBuilder();
            var players = mp3PlayerDfPlayerMiniConfigs ?? Array.Empty<Mp3PlayerDfPlayerMiniConfig>();
            // add  mp3 players using the constructor:  Mp3PlayerDfPlayerMiniConfig(int rxPin, int txPin, int volume, String name) 
            result.AppendLine($"\tmp3PlayersDfPlayerMini = new std::vector<Mp3PlayerDfPlayerMiniSerial>();");
            foreach (var player in players)
                result.AppendLine($"\tmp3PlayersDfPlayerMini->push_back(Mp3PlayerDfPlayerMiniSerial({player.RxPin}, {player.TxPin}, {player.Volume}, \"{player.Title}\", \"{player.Id}\", errorOccured));");

            result.AppendLine();
            return result.ToString();
        }

        /// <returns>null=ok, else the error message</returns>
     
    }
    public static class ExportStringBuilderExtensions
    {
        public static void AppendLines(this StringBuilder stringBuilder, IEnumerable<string> lines)
        {
            foreach (var line in lines)
                stringBuilder.AppendLine(line);

            stringBuilder.AppendLine();
        }

        public static string ToLines(this IEnumerable<string> lines)
        {
            var stringBuilder = new StringBuilder();
            foreach (var line in lines)
                stringBuilder.AppendLine(line);
            return stringBuilder.ToString();
        }
    }
}

// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License


using Awb.Core.Project;
using Awb.Core.Timelines;
using System.Text;

namespace Awb.Core.Export.ExporterParts
{
    internal class ProjectDataExporter : ExporterPartAbstract
    {
        private readonly ProjectExportData _projectData;

        public ProjectDataExporter(ProjectExportData projectData)
        {
            _projectData = projectData;
        }

        public override async Task<IExporter.ExportResult> ExportAsync(string targetSrcFolder)
        {
            var content = new StringBuilder();

            var includes = """
                #include <Arduino.h>
                #include <String.h>
                #include "../ProjectData/Timeline.h"
                #include "../ProjectData/TimelineState.h"
                #include "../ProjectData/StsServoPoint.h"
                #include "../ProjectData/Pca9685PwmServoPoint.h"
                #include "../ProjectData/Mp3PlayerYX5300Point.h"
                #include "../ProjectData/StsScsServo.h"
                #include "../ProjectData/Pca9685PwmServo.h"
                """;

            content.AppendLine(GetHeader(className: "ProjectData", includes: includes));

            content.AppendLine("public:");
            content.AppendLine($"   const char *ProjectName = \"{_projectData.ProjectName}\";");

            content.AppendLine();
            content.AppendLine($"   std::vector<StsScsServo> *scsServos;");
            content.AppendLine($"   std::vector<StsScsServo> *stsServos;");
            content.AppendLine($"   std::vector<Pca9685PwmServo> *pca9685PwmServos;");
            content.AppendLine($"   std::vector<Timeline>* timelines;");

            content.AppendLine();
        
            ExportMp3PlayerYX5300Informations(_projectData.Mp3PlayerYX5300Configs, content);
            ExportTimelineStates(timelineStates: _projectData.TimelineStates, content);
            ExportInputs(inputConfigs: _projectData.InputConfigs, content);
            content.AppendLine();

            content.AppendLine("ProjectData()");
            content.AppendLine("{");
            content.AppendLine();


            ExportStsScsServos(propertyName: "scsServos", servos: _projectData.ScsServoConfigs, content);
            ExportStsScsServos(propertyName: "stsServos", servos: _projectData.StsServoConfigs, content);
            ExportPCS9685PwmServos(_projectData.Pca9685PwmServoConfigs, content);
            content.AppendLine();

            content.AppendLine("   timelines = new std::vector<Timeline>();");
            content.AppendLine();

            var errMsg = ExportTimelineData(_projectData, content);
            if (errMsg != null)
                return new IExporter.ExportResult { ErrorMessage = errMsg };

            content.AppendLine();

            content.AppendLine("}");

            content.AppendLine(GetFooter("ProjectData"));


            if (!Directory.Exists(targetSrcFolder))
                return new IExporter.ExportResult { ErrorMessage = $"Target folder '{targetSrcFolder}' not found" };

            var folder = Path.Combine(targetSrcFolder, "src", "AwbDataImport");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            await File.WriteAllTextAsync(Path.Combine(folder, "ProjectData.h"), content.ToString());

            return IExporter.ExportResult.SuccessResult;
        }

        private static void ExportInputs(InputConfig[] inputConfigs, StringBuilder result)
        {
            // export the inputs
            var exportInputs = inputConfigs ?? Array.Empty<InputConfig>();
            result.AppendLine($"\tint inputIds[{exportInputs.Length}] = {{{string.Join(", ", exportInputs.Select(i => i.Id.ToString()))}}};");
            result.AppendLine($"\tString inputNames[{exportInputs.Length}] = {{{string.Join(", ", exportInputs.Select(s => $"\"{s.Name}\""))}}};");
            result.AppendLine($"\tuint8_t  inputIoPins[{exportInputs.Length}] = {{{string.Join(", ", exportInputs.Select(s => s.IoPin.ToString()))}}};");
            result.AppendLine($"\tint inputCount = {exportInputs.Length};");
            result.AppendLine();
        }

        private static void ExportTimelineStates(TimelineState[] timelineStates, StringBuilder result)
        {
            // export the states
            var exportStates = timelineStates?.Where(s => s.Export).ToArray() ?? Array.Empty<TimelineState>();
            var stateIds = exportStates.OrderBy(s => s.Id).Select(s => s.Id).ToArray() ?? Array.Empty<int>();
            result.AppendLine($"\tint timelineStateIds[{exportStates.Length}] = {{{string.Join(", ", stateIds)}}};");
            result.AppendLine($"\tString timelineStateNames[{exportStates.Length}] = {{{string.Join(", ", exportStates.Select(s => $"\"{s.Title}\""))}}};");
            result.AppendLine($"\tbool timelineStateAutoPlay[{exportStates.Length}] = {{{string.Join(", ", exportStates.Select(s => $"{s.AutoPlay.ToString().ToLower()}"))}}};");
            result.AppendLine($"\tint timelineStatePositiveInput[{exportStates.Length}] = {{{string.Join(", ", exportStates.Select(s => (s.PositiveInputs.FirstOrDefault()).ToString()))}}};");
            result.AppendLine($"\tint timelineStateNegativeInput[{exportStates.Length}] =  {{{string.Join(", ", exportStates.Select(s => (s.NegativeInputs.FirstOrDefault()).ToString()))}}};");
            result.AppendLine($"\tint timelineStateCount = {stateIds.Length};");
            result.AppendLine();
        }

        private static void ExportStsScsServos(string propertyName, StsServoConfig[] servos, StringBuilder result)
        {
            result.AppendLine($"   {propertyName} = new std::vector<StsScsServo>();");
            foreach (var servo in servos)
                // int channel, String const name, int minValue, int maxValue, int defaultValue, int acceleration, int speed, bool globalFault
                result.AppendLine($"   {propertyName}->push_back(StsScsServo({servo.Channel}, \"{servo.Title}\", {servo.MinValue}, {servo.MaxValue}, {servo.DefaultValue}, {servo.Acceleration}, {servo.Speed}, {servo.GlobalFault.ToString().ToLower()} ));");
            result.AppendLine();
        }

        private static void ExportPCS9685PwmServos(Pca9685PwmServoConfig[]? pca9685PwmServoConfigs, StringBuilder result)
        {
            var pca9685PwmServos = pca9685PwmServoConfigs?.OrderBy(s => s.Channel).ToArray() ?? Array.Empty<Project.Pca9685PwmServoConfig>();

            var propertyName = "pca9685PwmServos";
            result.AppendLine($"   {propertyName} = new std::vector<Pca9685PwmServo>();");

                foreach (var servo in pca9685PwmServos)
                    // int channel, String const name, int minValue, int maxValue, int defaultValue, int acceleration, int speed, bool globalFault
                    result.AppendLine($"   {propertyName}->push_back(Pca9685PwmServo({servo.I2cAdress}, {servo.Channel}, \"{servo.Title}\", {servo.MinValue}, {servo.MaxValue}, {servo.DefaultValue}));");

            result.AppendLine();
        }


        private static void ExportMp3PlayerYX5300Informations(Mp3PlayerYX5300Config[]? mp3PlayerYX5300Configs, StringBuilder result)
        {
            var players = mp3PlayerYX5300Configs ?? Array.Empty<Mp3PlayerYX5300Config>();
            var mp3PlayerYX5300Names = players?.Select(s => s.Name ?? s.SoundPlayerId).ToArray();
            result.AppendLine($"\tint mp3PlayerYX5300Count = {players!.Length};");
            result.AppendLine($"\tint mp3PlayerYX5300RxPin[{players.Length}] = {{{string.Join(", ", players.Select(s => s.RxPin.ToString()))}}};");
            result.AppendLine($"\tint mp3PlayerYX5300TxPin[{players.Length}] = {{{string.Join(", ", players.Select(s => s.TxPin.ToString()))}}};");
            result.AppendLine($"\tString mp3PlayerYX5300Name[{players.Length}] = {{{string.Join(", ", players.Select(s => $"\"{s.SoundPlayerId}\""))}}};");
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

                // Export Sound-Points
                foreach (var soundPoint in timeline.Points.OfType<SoundPoint>().OrderBy(p => p.TimeMs))
                {
                    if (projectData.Mp3PlayerYX5300Configs != null)
                    {
                        // find Mp3PlayerYX5300 soundplayer
                        var soundPlayerIndex = -1;
                        for (int i = 0; i < projectData.Mp3PlayerYX5300Configs?.Length; i++)
                        {
                            if (projectData.Mp3PlayerYX5300Configs[i].SoundPlayerId == soundPoint.SoundPlayerId)
                            {
                                soundPlayerIndex = i;
                                break;
                            }
                        }
                        if (soundPlayerIndex != -1)
                        {
                            var soundPlayer = projectData.Mp3PlayerYX5300Configs![soundPlayerIndex];
                            if (soundPlayer != null)
                            {
                                result.AppendLine($"\t\tmp3PlayerYX5300Points{timelineNo}->push_back(Mp3PlayerYX5300Point({soundPoint.SoundId}, {soundPlayerIndex}, {soundPoint.TimeMs}));");
                                continue;
                            }
                        }
                    }

                    // todo: find other sound player

                    // servo id not sound
                    return $"Soundplayer id '{soundPoint.SoundPlayerId}' not found in project config!";
                }

                result.AppendLine($"\t\tauto state{timelineNo} = new TimelineState({state.Id}, String(\"{state.Title}\"));");
                result.AppendLine($"\t\tTimeline *timeline{timelineNo} = new Timeline(state{timelineNo}, String(\"{timeline.Title}\"), stsServoPoints{timelineNo}, scsServoPoints{timelineNo}, pca9685PwmServoPoints{timelineNo}, mp3PlayerYX5300Points{timelineNo});");
                result.AppendLine($"\t\ttimelines->push_back(*timeline{timelineNo});");

                result.AppendLine();

                timelineNo++;
            }
            return null;
        }

    }
}

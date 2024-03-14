// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Project;
using System.Text;

namespace Awb.Core.LoadNSave.Export
{
    public class Esp32DataExporter
    {
        const string _intro = """
            #ifndef _AUTOPLAYDATA_H_
            #define _AUTOPLAYDATA_H_
            
            #include <Arduino.h>
            #include <String.h>
            #include "Timeline.h"
            #include "TimelineState.h"
            #include "StsServoPoint.h"
            #include "Pca9685PwmServoPoint.h"
            
            /// <summary>
            /// This is a prototype for the AutoPlayData class.
            /// It is used as a template to inject the generated data export of the animatronic workbench studio app.
            /// </summary>
            
            // Created with Animatronic Workbench Studio
            // https://daniel.springwald.de/post/AnimatronicWorkbench
            
            """;

        const string _head1 = """
           
            class AutoPlayData
            {

            protected:
            public:
            """;

        const string _head2 = """
                std::vector<Timeline> *timelines;

                AutoPlayData()
                {
                    timelines = new std::vector<Timeline>();
            """;

        const string _bottom = """
                }

                ~AutoPlayData()
                {
                }
            };

            #endif
            """;


        public Esp32ExportResult GetExportCode(Esp32ClientExportData exportData)
        {
            if (exportData.TimelineData == null) return new Esp32ExportResult { Ok = false, Message = "No timeline data to export" };

            var result = new StringBuilder();
            result.AppendLine(_intro);
            result.AppendLine($"// Created on {DateTime.Now.ToString()}");
            result.AppendLine(_head1);


            result.AppendLine($"\tconst char *ProjectName = \"{exportData.ProjectName}\";   // Project Name");
            result.AppendLine($"\tconst char *WlanSSID = \"AWB-{exportData.ProjectName}\";  // WLAN SSID Name");
            result.AppendLine($"\tconst char *WlanPassword = \"awb12345\"; // WLAN Password");
            result.AppendLine();

            // export the sts servo informations
            ExportScsStsServoInformations(servoConfigs: exportData.StsServoConfigs, praefix: "sts", result: result);

            // export the scs servo informations
            ExportScsStsServoInformations(servoConfigs: exportData.ScsServoConfigs, praefix: "scs", result: result);

            // export the pca9685 pwm servo informations
            ExportPCS9685PwmServoInformations(pca9685PwmServoConfigs: exportData.Pca9685PwmServoConfigs, result: result);

            // export the sound player informations
            ExportMp3PlayerYX5300Informations(mp3PlayerYX5300Configs: exportData.Mp3PlayerYX5300Configs, result: result);

            // export the states
            var exportStates = exportData.TimelineStates?.Where(s => s.Export).ToArray() ?? Array.Empty<TimelineState>();
            var stateIds = exportStates.OrderBy(s => s.Id).Select(s => s.Id).ToArray() ?? Array.Empty<int>();
            result.AppendLine($"\tint timelineStateIds[{exportStates.Length}] = {{{string.Join(", ", stateIds)}}};");
            result.AppendLine($"\tString timelineStateNames[{exportStates.Length}] = {{{string.Join(", ", exportStates.Select(s => $"\"{s.Name}\""))}}};");
            result.AppendLine($"\tint timelineStateCount = {stateIds.Length};");

            result.AppendLine(_head2);

            int timelineNo = 1;
            var timelines = exportData.TimelineData.OrderBy(t => t.TimelineStateId).ThenBy(t => t.Title).ToArray();
            foreach (var timeline in timelines)
            {
                var state = exportData.TimelineStates?.SingleOrDefault(x => x.Id == timeline.TimelineStateId);
                
                if (state == null)
                {
                    return new Esp32ExportResult
                    {
                        Ok = false,
                        Message = $"Timeline '{timeline.Title}' uses an undefined timelineStateId '{timeline.TimelineStateId}",
                    };
                }

                if (state.Export == false) continue;

                result.AppendLine($"\t\tauto *stsServoPoints{timelineNo} = new std::vector<StsServoPoint>();");
                result.AppendLine($"\t\tauto *scsServoPoints{timelineNo} = new std::vector<StsServoPoint>();");
                result.AppendLine($"\t\tauto *pca9685PwmServoPoints{timelineNo} = new std::vector<Pca9685PwmServoPoint>();");

                // Export Servo-Points
                foreach (var servoPoint in timeline.ServoPoints.OrderBy(p => p.TimeMs))
                {
                    // find STS servo
                    var stsServo = exportData.StsServoConfigs?.SingleOrDefault(s => s.Id == servoPoint.ServoId);
                    if (stsServo != null)
                    {
                        var value = (int)(stsServo.MinValue + servoPoint.ValuePercent * (stsServo.MaxValue - stsServo.MinValue) / 100.0);
                        result.AppendLine($"\t\tstsServoPoints{timelineNo}->push_back(StsServoPoint({stsServo.Channel},{servoPoint.TimeMs},{value}));");
                        continue;
                    }

                    // find SCS servo
                    var scsServo = exportData.ScsServoConfigs?.SingleOrDefault(s => s.Id == servoPoint.ServoId);
                    if (scsServo != null)
                    {
                        var value = (int)(scsServo.MinValue + servoPoint.ValuePercent * (scsServo.MaxValue - scsServo.MinValue) / 100.0);
                        result.AppendLine($"\t\tscsServoPoints{timelineNo}->push_back(StsServoPoint({scsServo.Channel},{servoPoint.TimeMs},{value}));");
                        continue;
                    }

                    // find pwm servo
                    var pwmServo = exportData.Pca9685PwmServoConfigs?.SingleOrDefault(s => s.Id == servoPoint.ServoId);
                    if (pwmServo != null)
                    {
                        var value = (int)(pwmServo.MinValue + servoPoint.ValuePercent * (pwmServo.MaxValue - pwmServo.MinValue) / 100.0);
                        result.AppendLine($"\t\tpca9685PwmServoPoints{timelineNo}->push_back(Pca9685PwmServoPoint({pwmServo.I2cAdress},{pwmServo.Channel},{servoPoint.TimeMs},{value}));");
                        continue;
                    }

                    // todo: find other servos

                    // servo id not sound
                    return new Esp32ExportResult
                    {
                        Ok = false,
                        Message = $"Servo id '{servoPoint.ServoId}' not found in project config!",
                    };
                }

                result.AppendLine($"\t\tauto state{timelineNo} = new TimelineState({state.Id}, String(\"{state.Name}\"));");
                result.AppendLine($"\t\tTimeline *timeline{timelineNo} = new Timeline(state{timelineNo}, String(\"{timeline.Title}\"), stsServoPoints{timelineNo}, scsServoPoints{timelineNo}, pca9685PwmServoPoints{timelineNo});");
                result.AppendLine($"\t\ttimelines->push_back(*timeline{timelineNo});");

                result.AppendLine();

                timelineNo++;
            }

            result.AppendLine(_bottom);
            return new Esp32ExportResult
            {
                Ok = true,
                Code = result.ToString()
            };
        }

        private void ExportMp3PlayerYX5300Informations(Mp3PlayerYX5300Config[]? mp3PlayerYX5300Configs, StringBuilder result)
        {
        }

        private static void ExportPCS9685PwmServoInformations(Pca9685PwmServoConfig[]? pca9685PwmServoConfigs, StringBuilder result)
        {
            var pca9685PwmServos = pca9685PwmServoConfigs?.OrderBy(s => s.Channel).ToArray() ?? Array.Empty<Project.Pca9685PwmServoConfig>();
            var pca9685PwmServoChannels = pca9685PwmServos.Select(s => s.Channel).ToArray();
            var pca9685PwmServoI2cAdresses = pca9685PwmServos.Select(s => s.I2cAdress).ToArray();
            var pca9685PwmServoAccelerations = pca9685PwmServos.Select(s => s.Acceleration ?? -1).ToArray();
            var pca9685PwmServoSpeeds = pca9685PwmServos.Select(s => s.Speed ?? -1).ToArray();
            var pca9685PwmServoNames = pca9685PwmServos.Select(s => s.Name ?? $"{s.Id}/{s.Channel}").ToArray();
            result.AppendLine($"\tint pca9685PwmServoCount = {pca9685PwmServos.Length};");
            result.AppendLine($"\tint pca9685PwmServoI2cAdresses[{pca9685PwmServos.Length}] = {{{string.Join(", ", pca9685PwmServoI2cAdresses.Select(s => s.ToString()))}}};");
            result.AppendLine($"\tint pca9685PwmServoChannels[{pca9685PwmServos.Length}] = {{{string.Join(", ", pca9685PwmServoChannels.Select(s => s.ToString()))}}};");
            result.AppendLine($"\tint pca9685PwmServoAccelleration[{pca9685PwmServos.Length}] = {{{string.Join(", ", pca9685PwmServoAccelerations.Select(s => s.ToString()))}}};");
            result.AppendLine($"\tint pca9685PwmServoSpeed[{pca9685PwmServos.Length}] = {{{string.Join(", ", pca9685PwmServoSpeeds.Select(s => s.ToString()))}}};");
            result.AppendLine($"\tString pca9685PwmServoName[{pca9685PwmServos.Length}] = {{{string.Join(", ", pca9685PwmServoNames.Select(s => $"\"{s}\""))}}};");
            result.AppendLine();
        }

        private static void ExportScsStsServoInformations(StsServoConfig[]? servoConfigs, string praefix, StringBuilder result)
        {
            var stsServos = servoConfigs?.OrderBy(s => s.Channel).ToArray() ?? Array.Empty<Project.StsServoConfig>();
            var chanels = stsServos.Select(s => s.Channel).ToArray();
            var accelerations = stsServos.Select(s => s.Acceleration ?? -1).ToArray();
            var speeds = stsServos.Select(s => s.Speed ?? -1).ToArray();
            var names = stsServos.Select(s => s.Name ?? $"{s.Id}/{s.Channel}").ToArray();
            result.AppendLine($"\tint {praefix}ServoCount = {stsServos.Length};");
            result.AppendLine($"\tint {praefix}ServoChannels[{stsServos.Length}] = {{{string.Join(", ", chanels.Select(s => s.ToString()))}}};");
            result.AppendLine($"\tint {praefix}ServoAcceleration[{stsServos.Length}] = {{{string.Join(", ", accelerations.Select(s => s.ToString()))}}};");
            result.AppendLine($"\tint {praefix}ServoSpeed[{stsServos.Length}] = {{{string.Join(", ", speeds.Select(s => s.ToString()))}}};");
            result.AppendLine($"\tString {praefix}ServoName[{stsServos.Length}] = {{{string.Join(", ", names.Select(s => $"\"{s}\""))}}};");
            result.AppendLine();
        }
    }
}

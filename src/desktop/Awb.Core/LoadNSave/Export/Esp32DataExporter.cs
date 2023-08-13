// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

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
                std::vector<Timeline> *timelines;
            """;

        const string _head2 = """
           
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

            var stsServos = exportData.StsServoConfigs?.OrderBy(s => s.Channel).ToArray() ?? Array.Empty<Configs.StsServoConfig>();
            var stsServoChannels = stsServos.Select(s => s.Channel).ToArray() ;
            var stsServoAccelerations = stsServos.Select(s => s.Accelleration  ?? -1).ToArray();
            var stsServoSpeeds = stsServos.Select(s => s.Speed ?? -1).ToArray();
            result.AppendLine($"\tint stsServoCount = {stsServos.Length};");
            result.AppendLine($"\tint stsServoChannels[{stsServos.Length}] = {{{string.Join(", ", stsServoChannels.Select(s => s.ToString()))}}};");
            result.AppendLine($"\tint stsServoAccelleration[{stsServos.Length}] = {{{string.Join(", ", stsServoAccelerations.Select(s => s.ToString()))}}};");
            result.AppendLine($"\tint stsServoSpeed[{stsServos.Length}] = {{{string.Join(", ", stsServoSpeeds.Select(s => s.ToString()))}}};");

            var stateIds = exportData.TimelineStates?.OrderBy(s => s.Id).Select(s => s.Id).ToArray() ?? Array.Empty<int>();
            result.AppendLine($"\tint timelineStateIds[{exportData.TimelineStates?.Length ?? 0}] = {{{string.Join(", ", stateIds)}}};");
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

                result.AppendLine($"\t\tauto *stsServoPoints{timelineNo} = new std::vector<StsServoPoint>();");

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

                    // find other servos
                    // todo eg. PWM Servos

                    // servo id not sound
                    return new Esp32ExportResult
                    {
                        Ok = false,
                        Message = $"Servo id '{servoPoint.ServoId}' not found in project config!",
                    };
                }

                result.AppendLine($"\t\tauto state{timelineNo} = new TimelineState({state.Id}, String(\"{state.Name}\"));");
                result.AppendLine($"\t\tTimeline *timeline{timelineNo} = new Timeline(state{timelineNo}, String(\"{timeline.Title}\"), stsServoPoints{timelineNo});");
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
    }
}

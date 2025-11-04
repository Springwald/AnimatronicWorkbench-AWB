// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Export.ExporterParts.ExportData;
using Awb.Core.Timelines;
using Awb.Core.Timelines.Sounds;
using System.Text;

namespace Awb.Core.Export.ExporterParts
{

    public class TimelineDataExporter
    {
        public LocalExportResult ExportTimelineData(ProjectExportData projectData)
        {
            var result = new StringBuilder();

            int timelineNo = 1;
            var timelines = projectData.TimelineData.OrderBy(t => t.TimelineStateId).ThenBy(t => t.Title).ToArray();
            foreach (var timeline in timelines)
            {
                var state = projectData.TimelineStates?.SingleOrDefault(x => x.Id == timeline.TimelineStateId);

                if (state == null)
                    return new LocalExportResult
                    {
                        ErrorMessage = $"Timeline '{timeline.Title}' uses an undefined timelineStateId '{timeline.TimelineStateId}"
                    };

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
                    return new LocalExportResult { ErrorMessage = $"Servo id '{servoPoint.ServoId}' not found in project config!" };
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
                    return new LocalExportResult { ErrorMessage = $"Soundplayer id '{soundPoint.SoundPlayerId}' not found in project config!" };
                }

                result.AppendLine($"\t\tauto state{timelineNo} = new TimelineStateReference({state.Id}, String(\"{state.Title}\"));");
                result.AppendLine($"\t\tTimeline *timeline{timelineNo} = new Timeline(state{timelineNo}, {timeline.NextTimelineStateOnceId ?? -1}, String(\"{timeline.Title}\"), stsServoPoints{timelineNo}, scsServoPoints{timelineNo}, pca9685PwmServoPoints{timelineNo}, mp3PlayerYX5300Points{timelineNo}, mp3PlayerDfPlayerMiniPoints{timelineNo});");
                result.AppendLine($"\t\ttimelines->push_back(*timeline{timelineNo});");

                result.AppendLine();

                timelineNo++;
            }
            return new LocalExportResult { Content = result.ToString() };
        }
    }
}

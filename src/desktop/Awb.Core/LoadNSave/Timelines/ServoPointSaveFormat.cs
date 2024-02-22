// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Timelines;

namespace Awb.Core.LoadNSave.Timelines
{
    public class ServoPointSaveFormat
    {
        public int TimeMs { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string TargetObjectId { get; set; }
        public double ValuePercent { get; set; }
        public string ServoId { get; set; }

        public ServoPointSaveFormat(int timeMs, string targetObjectId, double valuePercent, string servoId)
        {
            TimeMs = timeMs;
            TargetObjectId = targetObjectId;
            ValuePercent = valuePercent;
            ServoId = servoId;
        }

        public static ServoPointSaveFormat FromServoPoint(ServoPoint servoPoint) => new ServoPointSaveFormat(
                timeMs: servoPoint.TimeMs,
                targetObjectId: servoPoint.TargetObjectId,
                valuePercent: ((ServoPoint)servoPoint).ValuePercent,
                servoId: ((ServoPoint)servoPoint).ServoId)
                    {
                        Title = servoPoint.Title,
                        Description = servoPoint.Description,
                    };

        public static ServoPoint ToServoPoint(ServoPointSaveFormat servoPoint) =>
                new ServoPoint(servoId: servoPoint.ServoId, valuePercent: servoPoint.ValuePercent, timeMs: servoPoint.TimeMs)
                {
                    Description = servoPoint.Description,
                };
    }
}

// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

namespace Awb.Core.Timelines
{
    public class ServoPoint : TimelinePoint
    {
        public string ServoId { get; set; }

        /// <summary>
        /// percent in range 0..100 for servo.MinValue...servo.MaxValue
        /// </summary>
        public double ValuePercent { get; set; }

        public override string Title { get; set; }

        public override string PainterCheckSum => ServoId + base.TimeMs.ToString() + ValuePercent.ToString();

        /// <summary>
        /// Move the servo to this position at this time
        /// </summary>
        /// <param name="value">percent in range 0..100 for servo.MinValue...servo.MaxValue</param>
        public ServoPoint(string servoId, double valuePercent, int timeMs) : base(targetObjectId: servoId, timeMs: timeMs)
        {
            ServoId = servoId;
            ValuePercent = valuePercent;
            Title = $"{ServoId}: {ValuePercent:0.0}% {TimeMs}ms";
        }

        public override ServoPoint Clone()
        {
            return new ServoPoint(servoId: ServoId, valuePercent: ValuePercent, timeMs: TimeMs) { Title = Title };
        }
    }
}

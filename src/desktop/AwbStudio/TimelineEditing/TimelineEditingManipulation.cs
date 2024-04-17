// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Actuators;
using Awb.Core.Player;
using Awb.Core.Timelines;
using System.Linq;

namespace AwbStudio.TimelineEditing
{
    internal class TimelineEditingManipulation
    {
        private readonly TimelineData _timelineData;
        private readonly PlayPosSynchronizer _playPosSynchronizer;

        public TimelineEditingManipulation(TimelineData timelineData, TimelineViewContext? _viewContext, PlayPosSynchronizer playPosSynchronizer)
        {
            _timelineData = timelineData;
            _playPosSynchronizer = playPosSynchronizer;
        }

        public void UpdateServoValue(IServo servo, double targetPercent)
        {
            var servoPoint = _timelineData?.ServoPoints.OfType<ServoPoint>().SingleOrDefault(p => p.ServoId == servo.Id && (int)p.TimeMs == _playPosSynchronizer.PlayPosMs); // check existing point
            if (servoPoint == null)
            {
                servoPoint = new ServoPoint(servo.Id, targetPercent, _playPosSynchronizer.PlayPosMs);
                _timelineData?.ServoPoints.Add(servoPoint);
            }
            else
            {
                servoPoint.ValuePercent = targetPercent;
            }
            _timelineData!.SetContentChanged(TimelineDataChangedEventArgs.ChangeTypes.ServoPointChanged, servoPoint.ServoId);
        }
    }

}

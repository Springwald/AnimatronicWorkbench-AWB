// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.LoadNSave.Timelines;
using Awb.Core.Timelines;

namespace Awb.Core.LoadNSave.TimelineLoadNSave
{
    public class TimelineSaveFormat
    {

        public int TimelineStateId { get; set; }

        public ServoPointSaveFormat[]? ServoPoints { get; set; }
        public SoundPointSaveFormat[]? SoundPoints { get; private set; }

        public static TimelineSaveFormat FromTimelineData(TimelineData timelineData)
        {
            return new TimelineSaveFormat
            {
                TimelineStateId = timelineData.TimelineStateId,
                ServoPoints = timelineData.ServoPoints.Select(p => ServoPointSaveFormat.FromServoPoint(p)).OrderBy(p => p.TimeMs).ToArray(),
                SoundPoints = timelineData.SoundPoints.Select(p => SoundPointSaveFormat.FromSoundPoint(p)).OrderBy(p => p.TimeMs).ToArray()
            };
        }

        public static TimelineData ToTimelineData(TimelineSaveFormat saveFormat)
        {
            return new TimelineData(
                timelineStateId: saveFormat.TimelineStateId,
                servoPoints: saveFormat?.ServoPoints?.Select(p => ServoPointSaveFormat.ToServoPoint(p)).ToList() ?? new List<ServoPoint>(),
                soundPoints: saveFormat?.SoundPoints?.Select(p => SoundPointSaveFormat.ToSoundPoint(p)).ToList() ?? new List<SoundPoint>());
        }
    }
}

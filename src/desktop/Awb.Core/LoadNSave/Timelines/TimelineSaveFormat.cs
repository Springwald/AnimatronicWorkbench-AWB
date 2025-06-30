// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.LoadNSave.Timelines;
using Awb.Core.Timelines;
using Awb.Core.Timelines.NestedTimelines;
using Awb.Core.Timelines.Sounds;

namespace Awb.Core.LoadNSave.TimelineLoadNSave
{
    public class TimelineSaveFormat
    {
        public required string Title { get; set; }

        public int TimelineStateId { get; set; }
        public int? NextTimelineStateIdOnce { get; set; }

        public ServoPointSaveFormat[]? ServoPoints { get; set; }
        public SoundPointSaveFormat[]? SoundPoints { get; set; }
        public NestedTimelinePointSaveFormat[]? NestedTimelinePoints { get; set; }


        public static TimelineSaveFormat FromTimelineData(TimelineData timelineData)
        {
            return new TimelineSaveFormat
            {
                Title = timelineData.Title,
                TimelineStateId = timelineData.TimelineStateId,
                NextTimelineStateIdOnce = timelineData.NextTimelineStateIdOnce,
                ServoPoints = timelineData.ServoPoints.Select(p => ServoPointSaveFormat.FromServoPoint(p)).OrderBy(p => p.TimeMs).ToArray(),
                SoundPoints = timelineData.SoundPoints.Select(p => SoundPointSaveFormat.FromSoundPoint(p)).OrderBy(p => p.TimeMs).ToArray(),
                NestedTimelinePoints = timelineData.NestedTimelinePoints.Select(p => NestedTimelinePointSaveFormat.FromNestedTimelinePoint(p)).OrderBy(p => p.TimeMs).ToArray()
            };
        }

        public static TimelineData ToTimelineData(TimelineSaveFormat saveFormat, string timelineId)
        {
            return new TimelineData(
                id: timelineId.ToString(),
                timelineStateId: saveFormat.TimelineStateId,
                nextTimelineStateIdOnce: saveFormat.NextTimelineStateIdOnce,
                servoPoints: saveFormat.ServoPoints?.Select(p => ServoPointSaveFormat.ToServoPoint(p)).ToList() ?? new List<ServoPoint>(),
                soundPoints: saveFormat.SoundPoints?.Select(p => SoundPointSaveFormat.ToSoundPoint(p)).ToList() ?? new List<SoundPoint>(),
                nestedTimelinePoints: saveFormat.NestedTimelinePoints?.Select(p => NestedTimelinePointSaveFormat.ToNestedTimelinePoint(p)).ToList() ?? new List<NestedTimelinePoint>()
                )
            { Title = saveFormat.Title };
        }
    }
}

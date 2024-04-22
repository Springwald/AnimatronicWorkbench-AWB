// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.LoadNSave.Timelines;
using Awb.Core.Timelines;
using Awb.Core.Timelines.NestedTimelines;

namespace Awb.Core.LoadNSave.TimelineLoadNSave
{
    public class TimelineSaveFormat
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public int TimelineStateId { get; set; }

        public ServoPointSaveFormat[]? ServoPoints { get; set; }
        public SoundPointSaveFormat[]? SoundPoints { get; set; }
        public NestedTimelinePointSaveFormat[]? NestedTimelinePoints { get; set; }


        public static TimelineSaveFormat FromTimelineData(TimelineData timelineData)
        {
            return new TimelineSaveFormat
            {
                Id = timelineData.Id,
                Title = timelineData.Title,
                TimelineStateId = timelineData.TimelineStateId,
                ServoPoints = timelineData.ServoPoints.Select(p => ServoPointSaveFormat.FromServoPoint(p)).OrderBy(p => p.TimeMs).ToArray(),
                SoundPoints = timelineData.SoundPoints.Select(p => SoundPointSaveFormat.FromSoundPoint(p)).OrderBy(p => p.TimeMs).ToArray(),
                NestedTimelinePoints = timelineData.NestedTimelinePoints.Select(p => NestedTimelinePointSaveFormat.FromNestedTimelinePoint(p)).OrderBy(p => p.TimeMs).ToArray()
            };
        }

        public static TimelineData ToTimelineData(TimelineSaveFormat saveFormat)
        {
            return new TimelineData(
                id: saveFormat.Id,
                timelineStateId: saveFormat.TimelineStateId,
                servoPoints: saveFormat.ServoPoints?.Select(p => ServoPointSaveFormat.ToServoPoint(p)).ToList() ?? new List<ServoPoint>(),
                soundPoints: saveFormat.SoundPoints?.Select(p => SoundPointSaveFormat.ToSoundPoint(p)).ToList() ?? new List<SoundPoint>(),
                nestedTimelinePoints: saveFormat.NestedTimelinePoints?.Select(p => NestedTimelinePointSaveFormat.ToNestedTimelinePoint(p)).ToList() ?? new List<NestedTimelinePoint>()
                )
            { Title = saveFormat.Title };
        }
    }
}

// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Timelines;

namespace Awb.Core.LoadNSave.Timelines
{
    public class SoundPointSaveFormat
    {
        public int TimeMs { get; }
        public string TargetObjectId { get; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int SoundIndex { get; set; }

        public SoundPointSaveFormat(int timeMs, string targetObjectId, int soundIndex)
        {
            TimeMs = timeMs;
            TargetObjectId = targetObjectId;
            SoundIndex = soundIndex;
        }

        public static SoundPointSaveFormat FromSoundPoint(SoundPoint soundPoint) => new SoundPointSaveFormat(
                timeMs: soundPoint.TimeMs,
                targetObjectId: soundPoint.TargetObjectId,
                soundIndex: soundPoint.SoundIndex
                )
        {
            Title = soundPoint.Title,
            Description = soundPoint.Description,
        };

        public static SoundPoint ToSoundPoint(SoundPointSaveFormat soundPointSaveFormat) =>
                new SoundPoint(timeMs: soundPointSaveFormat.TimeMs, soundIndex: soundPointSaveFormat.SoundIndex)
                {
                    Description = soundPointSaveFormat.Description,
                };
    }
}

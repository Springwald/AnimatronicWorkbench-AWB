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
        public int TimeMs { get; set;  }
        public string TargetObjectId { get; set; }
        public string SoundPlayerId => TargetObjectId;
        public string Title { get; set; }
        public string? Description { get; set; }
        public int SoundId { get; set; }

        public SoundPointSaveFormat(int timeMs, string targetObjectId, int soundId)
        {
            TimeMs = timeMs;
            TargetObjectId = targetObjectId;
            SoundId = soundId;
        }

        public static SoundPointSaveFormat FromSoundPoint(SoundPoint soundPoint) => new SoundPointSaveFormat(
                timeMs: soundPoint.TimeMs,
                targetObjectId: soundPoint.SoundPlayerId,
                soundId: soundPoint.SoundId
                )
        {
            Title = soundPoint.Title,
            Description = soundPoint.Description,
        };

        public static SoundPoint ToSoundPoint(SoundPointSaveFormat soundPointSaveFormat)
        => new SoundPoint(
            timeMs: soundPointSaveFormat.TimeMs,
            soundPlayerId: soundPointSaveFormat.TargetObjectId,
            title: soundPointSaveFormat.Title,
            soundId: soundPointSaveFormat.SoundId)
        {
            Description = soundPointSaveFormat.Description,
        };
    }
}

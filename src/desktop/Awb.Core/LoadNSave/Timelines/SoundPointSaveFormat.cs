// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Timelines;

namespace Awb.Core.LoadNSave.Timelines
{
    public class SoundPointSaveFormat
    {

        public int TimeMs { get; }
        public string TargetObjectId { get; }
        public string? Title { get; private set; }
        public string? Description { get; private set; }

        public SoundPointSaveFormat(int timeMs, string targetObjectId)
        {
            TimeMs = timeMs;
            TargetObjectId = targetObjectId;
        }

        public static SoundPointSaveFormat FromSoundPoint(SoundPoint soundPoint) => new SoundPointSaveFormat(
                timeMs: soundPoint.TimeMs,
                targetObjectId: soundPoint.TargetObjectId
         )
        {
            Title = soundPoint.Title,
            Description = soundPoint.Description,
        };

        public static SoundPoint ToSoundPoint(SoundPointSaveFormat soundPoint) =>
                new SoundPoint(timeMs: soundPoint.TimeMs, soundId: SoundPoint.SoundPlayerObjectId)
                {
                    Description = soundPoint.Description,
                    Title = soundPoint.Title,
                };
    }
}

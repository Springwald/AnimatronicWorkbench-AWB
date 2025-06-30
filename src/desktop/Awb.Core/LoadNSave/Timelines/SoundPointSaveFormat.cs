// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Timelines.Sounds;
using System.Text.Json.Serialization;

namespace Awb.Core.LoadNSave.Timelines
{
    public class SoundPointSaveFormat
    {
        public int TimeMs { get; set; }
        public required string TargetObjectId { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public int SoundId { get; set; }


        [JsonIgnore]
        public string SoundPlayerId => TargetObjectId;

        public MovementsActuatorsBySoundSaveFormat[]? MovementsActuatorsBySoundSaveFormats { get; set; } = Array.Empty<MovementsActuatorsBySoundSaveFormat>();

        public static SoundPointSaveFormat FromSoundPoint(SoundPoint soundPoint) => new SoundPointSaveFormat
        {
            TimeMs = soundPoint.TimeMs,
            TargetObjectId = soundPoint.SoundPlayerId,
            SoundId = soundPoint.SoundId,
            Title = soundPoint.Title,
            Description = soundPoint.Description,
            MovementsActuatorsBySoundSaveFormats = soundPoint.ActuatorMovementsBySound.Select(movement => MovementsActuatorsBySoundSaveFormat.FromSoundPoint(movement)).ToArray()
        };

        public static SoundPoint ToSoundPoint(SoundPointSaveFormat soundPointSaveFormat)
            => new SoundPoint(
            timeMs: soundPointSaveFormat.TimeMs,
            soundPlayerId: soundPointSaveFormat.TargetObjectId,
            title: soundPointSaveFormat.Title,
            soundId: soundPointSaveFormat.SoundId,
            actuatorMovementsBySound: soundPointSaveFormat.MovementsActuatorsBySoundSaveFormats.Select(movement => MovementsActuatorsBySoundSaveFormat.ToSoundPoint(movement)).ToArray(
            ))
            {
                Description = soundPointSaveFormat.Description,
            };
    }
}

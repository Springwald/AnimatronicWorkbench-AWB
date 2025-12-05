// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Timelines;

namespace Awb.Core.LoadNSave.Timelines
{
    public class MovementsActuatorsBySoundSaveFormat
    {
        public string? ActuatorId { get; set; } = null;
        public bool MovementInverted { get; set; } = false;// up/down movement inverted, e.g. for a jaw servo
        public int MovementOffsetMs { get; set; } = 0; // offset in ms to the sound start, e.g. for moving a servo before the sound starts
        public int MovementFrequencyMs { get; set; } = 50; // frequency in ms tp create servo points
        public int MovementValueScale { get; set; } = 100; // scale for the movement value, e.g. 100 for a full range of 0-100, 50 for a half range of 0-50

        public MovementsActuatorsBySoundSaveFormat()
        {
        }

        public static MovementsActuatorsBySoundSaveFormat FromSoundPoint(ActuatorMovementBySound actuatorMovementBySound)
            => new MovementsActuatorsBySoundSaveFormat
            {
                ActuatorId = actuatorMovementBySound.ActuatorId,
                MovementInverted = actuatorMovementBySound.MovementInverted,
                MovementOffsetMs = actuatorMovementBySound.MovementOffsetMs,
                MovementFrequencyMs = actuatorMovementBySound.MovementFrequencyMs,
                MovementValueScale = actuatorMovementBySound.MovementValueScale
            };

        public static ActuatorMovementBySound ToSoundPoint(MovementsActuatorsBySoundSaveFormat soundPointSaveFormat)
         => new ActuatorMovementBySound
         {
             ActuatorId = soundPointSaveFormat.ActuatorId,
             MovementOffsetMs = soundPointSaveFormat.MovementOffsetMs,
             MovementFrequencyMs = soundPointSaveFormat.MovementFrequencyMs,
             MovementInverted = soundPointSaveFormat.MovementInverted,
             MovementValueScale = soundPointSaveFormat.MovementValueScale
         };
    }
}

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
        public string? ActuatorId { get; set; }
        public bool MovementInverted { get; set; } // up/down movement inverted, e.g. for a jaw servo
        public int MovementOffsetMs { get; set; } = 0; // offset in ms to the sound start, e.g. for moving a servo before the sound starts
        public int MovementFrequencyMs { get; set; } = 50; // frequency in ms tp create servo points

        public MovementsActuatorsBySoundSaveFormat(string? actuatorId = null, bool movementInverted = false, int movementOffsetMs = 0, int movementFrequencyMs = 50)
        {
            ActuatorId = actuatorId;
            MovementInverted = movementInverted;
            MovementOffsetMs = movementOffsetMs;
            MovementFrequencyMs = movementFrequencyMs;
        }

        public static MovementsActuatorsBySoundSaveFormat FromSoundPoint(ActuatorMovementBySound actuatorMovementBySound)
            => new MovementsActuatorsBySoundSaveFormat
            {
                ActuatorId = actuatorMovementBySound.ActuatorId,
                MovementInverted = actuatorMovementBySound.MovementInverted,
                MovementOffsetMs = actuatorMovementBySound.MovementOffsetMs,
                MovementFrequencyMs = actuatorMovementBySound.MovementFrequencyMs
            };

        public static ActuatorMovementBySound ToSoundPoint(MovementsActuatorsBySoundSaveFormat soundPointSaveFormat)
         => new ActuatorMovementBySound
         {
             ActuatorId = soundPointSaveFormat.ActuatorId,
             MovementOffsetMs = soundPointSaveFormat.MovementOffsetMs,
             MovementFrequencyMs = soundPointSaveFormat.MovementFrequencyMs,
             MovementInverted = soundPointSaveFormat.MovementInverted
         };
    }
}

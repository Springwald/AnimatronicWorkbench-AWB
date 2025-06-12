// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

namespace Awb.Core.Timelines.Sounds
{
    public class SoundPoint : TimelinePoint
    {
        /// <summary>
        /// The resource id of the sound to be played.
        /// What kind of resource this is depends on the implementation of the sound player.
        /// </summary>
        public int SoundId { get; set; }

        /// <summary>
        /// A specific sound player e.g. for multi puppet scenarios
        /// </summary>
        public string SoundPlayerId { get; set; }

        /// <summary>
        /// If actuators should move in sync with the sound, this array contains the actuator movements.
        /// </summary>
        /// <remarks>
        /// At the moment, only one movement per sound is supported by the visual editor.
        /// In the future, this might change to allow multiple movements per sound, e.g. if a mouth is controlled by multiple servos.
        /// </remarks>
        public ActuatorMovementBySound[] ActuatorMovementsBySound { get; set; } = Array.Empty<ActuatorMovementBySound>();

        public override string Title { get; }

        public override string PainterCheckSum => $"{SoundId}-{TimeMs}-{SoundPlayerId}-{string.Join(".", ActuatorMovementsBySound.Select(a => a.PainterChecksum))}";

        /// <param name="soundId">The resource id of the sound to be played. What kind of resource this is depends on the implementation of the sound player.</param>
        public SoundPoint(int timeMs, string soundPlayerId, string title, int soundId, ActuatorMovementBySound[] actuatorMovementsBySound) : base(targetObjectId: soundPlayerId, timeMs: timeMs)
        {
            SoundId = soundId;
            Title = title;
            SoundPlayerId = soundPlayerId;
            ActuatorMovementsBySound = actuatorMovementsBySound;
        }

        public override SoundPoint Clone()
        {
            var cloneMovements = new ActuatorMovementBySound[ActuatorMovementsBySound.Length];
            for (int i = 0; i < ActuatorMovementsBySound.Length; i++)
            {
                cloneMovements[i] = new ActuatorMovementBySound
                {
                    ActuatorId = ActuatorMovementsBySound[i].ActuatorId,
                    MovementInverted = ActuatorMovementsBySound[i].MovementInverted,
                    MovementOffsetMs = ActuatorMovementsBySound[i].MovementOffsetMs,
                    MovementFrequencyMs = ActuatorMovementsBySound[i].MovementFrequencyMs
                };
            }

            return new SoundPoint(timeMs:TimeMs, soundPlayerId: SoundPlayerId, title: Title, soundId: SoundId, actuatorMovementsBySound: cloneMovements);
        }

    }
}
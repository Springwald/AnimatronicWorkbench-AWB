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

        public string? MovementServoId { get; set; }
        public bool MovementInverted { get; set; } // up/down movement inverted, e.g. for a jaw servo

        public int MovementOffsetMs { get; } = 0; // offset in ms to the sound start, e.g. for moving a servo before the sound starts
        public int MovementFrequencyMs { get; } = 50; // frequency in ms tp create servo points


        public override string Title { get; }


        public override string PainterCheckSum => SoundId.ToString() + TimeMs.ToString() + SoundPlayerId.ToString();

        /// <param name="soundId">The resource id of the sound to be played. What kind of resource this is depends on the implementation of the sound player.</param>
        public SoundPoint(int timeMs, string soundPlayerId, string title, int soundId, string? movementServoId, bool movementInverted) : base(targetObjectId: soundPlayerId, timeMs: timeMs)
        {
            SoundId = soundId;
            Title = title;
            SoundPlayerId = soundPlayerId;
            MovementServoId = movementServoId;
            MovementInverted = movementInverted;
        }

        public override SoundPoint Clone()
        {
            return new SoundPoint(timeMs:TimeMs, soundPlayerId: SoundPlayerId, title: Title, soundId: SoundId, movementInverted: MovementInverted, movementServoId: MovementServoId);
        }

    }
}
// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

namespace Awb.Core.Timelines
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

        public override string Title { get; set; }

        public override string PainterCheckSum => SoundId.ToString() + base.TimeMs.ToString() + SoundPlayerId.ToString();

        /// <param name="soundId">The resource id of the sound to be played. What kind of resource this is depends on the implementation of the sound player.</param>
        public SoundPoint(int timeMs, string soundPlayerId, string title, int soundId) : base(targetObjectId: soundPlayerId, timeMs: timeMs)
        {
            this.SoundId = soundId;
            this.Title = title;
            this.SoundPlayerId = soundPlayerId;
        }
    }
}
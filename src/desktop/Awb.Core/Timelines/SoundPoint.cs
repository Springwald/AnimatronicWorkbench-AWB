// AnimatronicWorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

namespace Awb.Core.Timelines
{
    public class SoundPoint : TimelinePoint
    {
        public const string SoundPlayerObjectId = "snd";

        /// <summary>
        /// The resource id of the sound to be played.
        /// What kind of resource this is depends on the implementation of the sound player.
        /// </summary>
        public string SoundId { get; set; }

        /// <summary>
        /// A specific sound player e.g. for multi puppet scenarios
        /// </summary>
        public string? SoundPlayerId { get; set; }


        /// <param name="soundId">The resource id of the sound to be played. What kind of resource this is depends on the implementation of the sound player.</param>
        public SoundPoint(int timeMs, string soundId) : base(SoundPlayerObjectId, timeMs)
        {
            this.SoundId = soundId;
        }
    }
}

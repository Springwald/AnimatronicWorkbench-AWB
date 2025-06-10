// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

namespace Awb.Core.Player
{
    public class SoundPlayEventArgs : EventArgs
    {
        public int SoundId { get; set; }
        public TimeSpan? StartTime { get; set; } = null;

        public SoundPlayEventArgs(int soundId, TimeSpan? startTime)
        {
            SoundId = soundId;
            StartTime = startTime;
        }
    }
}

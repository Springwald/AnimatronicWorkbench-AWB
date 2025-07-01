// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License


using Awb.Core.Sounds;

namespace Awb.Core.Player
{
    public class SoundRequestEventArgs : EventArgs
    {
        public int SoundId { get; set; }
        public Sound? Sound { get; set; } = null;
        public SoundRequestEventArgs(int soundId)
        {
            SoundId = soundId;
        }
    }
}

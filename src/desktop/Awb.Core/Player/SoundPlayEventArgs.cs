// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Sounds;

namespace Awb.Core.Player
{
    public class SoundPlayEventArgs : EventArgs
    {
        public int SoundIndex { get; set; }

        public SoundPlayEventArgs(int soundIndex)
        {
            SoundIndex = soundIndex;
        }
    }
}

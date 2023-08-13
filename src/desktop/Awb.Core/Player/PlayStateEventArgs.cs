// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

namespace Awb.Core.Player
{
    public class PlayStateEventArgs : EventArgs
    {
        public TimelinePlayer.PlayStates PlayState { get; set; }
        public double PlaybackSpeed { get; set; }
        public int PositionMs { get; set; }
    }
}

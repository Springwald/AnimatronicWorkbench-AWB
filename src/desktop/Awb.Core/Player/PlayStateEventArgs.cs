// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

namespace Awb.Core.Player
{
    public class PlayStateEventArgs : EventArgs
    {
        public TimelinePlayer.PlayStates PlayState { get; set; }
        public double PlaybackSpeed { get; set; }
    }
}

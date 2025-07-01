// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

namespace Awb.Core.DataPackets
{
    public class AdafruitPwmChannel
    {
        public int Channel { get; set; }
        public int Value { get; set; }
        public string? Name { get; set; }
    }
}

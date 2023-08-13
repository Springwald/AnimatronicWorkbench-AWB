// AnimatronicWorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

namespace Awb.Core.DataPackets
{
    public class AdafruitPwmChannel
    {
        public int Channel { get; set; }
        public int Value { get; set; }
        public string? Name { get; set; }
    }
}

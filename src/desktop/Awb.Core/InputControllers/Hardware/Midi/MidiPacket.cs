// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

namespace Awb.Core.InputControllers.Midi
{
    public class MidiPacket
    {
        public MidiPacket(long timestamp, ushort length, IntPtr bytes)
        {
            this.TimeStamp = timestamp;
            this.Length = length;
            this.Bytes = bytes;
        }

        public int Length { get; private set; }

        public IntPtr Bytes { get; private set; }
        public long TimeStamp { get; internal set; }
    }
}

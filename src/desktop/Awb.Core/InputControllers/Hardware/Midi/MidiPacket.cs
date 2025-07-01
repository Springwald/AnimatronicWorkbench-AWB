// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

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

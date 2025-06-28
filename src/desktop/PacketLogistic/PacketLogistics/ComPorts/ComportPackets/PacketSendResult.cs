// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

namespace PacketLogistics.ComPorts.ComportPackets
{
    public class PacketSendResult
    {
        public int ClientId { get; internal set; }
        public bool Ok { get; internal set; }
        public string? Message { get; internal set; }
        public uint? OriginalPacketId { get; internal set; }
    }
}

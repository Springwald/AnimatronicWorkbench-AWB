// Communicate between different devices on dotnet or arduino via COM port or Wifi
// https://github.com/Springwald/PacketLogistics
//
// (C) 2023 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

namespace PacketLogistics.ComPorts.ComportPackets
{
    public class PacketSendResult
    {
        public int ClientId { get; internal set; }
        public bool Ok { get; internal set; }
        public string? Message { get; internal set; }
        public DateTime OriginalPacketTimestampUtc { get; internal set; }
        public uint? OriginalPacketId { get; internal set; }
    }
}

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
    internal class DataPacket : PacketBase
    {
        /// <summary>
        /// When was this packet created by the sender(!).
        /// In the full lifecycle of a packet, the timestamp of send packet and returned has the be identical
        /// </summary>
        public DateTime TimestampUtc { get; set; }

        public uint SenderId { get; set; }

        public byte[]? Payload { get; set; }

        public DataPacket(uint id)
        {
            base.Id = id;
            base.PacketType = PacketTypes.DataPacket;
        }
    }
}

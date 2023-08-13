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
    internal class ResponsePacket : PacketBase
    {
        private uint _packetId;

        public bool Ok { get; internal set; }
        public string? Message { get; internal set; }

 

        public ResponsePacket(uint id)
        {
            base.Id = id;
            base.PacketType = PacketTypes.ResponsePacket;
        }
    }
}

// Communicate between different devices on dotnet or arduino via COM port or Wifi
// https://github.com/Springwald/PacketLogistics
//
// (C) 2023 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using System.Reflection.PortableExecutable;

namespace PacketLogistics.ComPorts.ComportPackets
{
    internal class AlifePacket : PacketBase
    {
        public uint ClientId { get; }

        public AlifePacket(uint clientId)
        {
            base.PacketType = PacketTypes.AlifePacket;
            this.ClientId = clientId;
        }
       
    }
}

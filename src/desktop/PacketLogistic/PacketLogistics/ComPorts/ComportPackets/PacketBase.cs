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
    public abstract class PacketBase
    {
        private uint _packetId;

        public enum PacketTypes
        {
            AlifePacket = 1,
            DataPacket = 2,
            ResponsePacket = 3,
        }

        public PacketTypes PacketType { get; init; }

        /// <summary>
        /// The unique Id of a packet, counting up every packet
        /// </summary>
        public uint Id // { get; internal set; }
        //public uint OriginalPacketId
        {
            get => _packetId;
            protected set
            {
                if (value < 0 || value >= uint.MaxValue) throw new ArgumentOutOfRangeException(paramName: nameof(Id), message: $"Id must be >= 0 and <= {uint.MaxValue} but is " + value);
                _packetId = value;
            }
        }



    }
}

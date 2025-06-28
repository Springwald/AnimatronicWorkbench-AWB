//// Animatronic WorkBench
//// https://github.com/Springwald/AnimatronicWorkBench-AWB
////
//// (C) 2025 Daniel Springwald      -     Bochum, Germany
//// https://daniel.springwald.de - segfault@springwald.de
//// All rights reserved    -   Licensed under MIT License

//namespace PacketLogistics.ComPorts.ComportPackets
//{
//    public class Packet<PayloadTypes> where PayloadTypes : Enum
//    {
//        private uint _packetId;

//        /// <summary>
//        /// The unique Id of a packet, counting up every packet
//        /// </summary>
//        public required uint Id 
//        {
//            get => _packetId;
//            set
//            {
//                if (value < 0 || value >= uint.MaxValue) throw new ArgumentOutOfRangeException(paramName: nameof(Id), message: $"Id must be >= 0 and <= {uint.MaxValue} but is " + value);
//                _packetId = value;
//            }
//        }

//        /// <summary>
//        /// The optional payload of the packet.
//        /// </summary>
//        public required string? Payload { get; init; }

//        /// <summary>
//        /// The project specific type of the packet.
//        /// </summary>
//        public required PayloadTypes PayloadType { get; init; }
//    }
//}

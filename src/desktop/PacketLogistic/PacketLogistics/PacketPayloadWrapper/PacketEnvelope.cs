// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

namespace PacketLogistics.PacketPayloadWrapper
{
    internal class PacketEnvelope<PayloadTypes> where PayloadTypes : Enum
    {
        /// <summary>
        /// What type of packet is this?
        /// </summary>
        public enum PacketTypes
        {
            /// <summary>
            /// Fallback type for unknown packets.
            /// </summary>
            NoSet = 0,

            /// <summary>
            /// Send by a client to by scanned by the server to check if the client is available
            /// </summary>
            AlivePacket = 1,

            /// <summary>
            /// A packet that tells the server if a packet was received successfully or not
            /// </summary>
            ResponsePacket = 2,

            /// <summary>
            /// A packet that contains data to be processed by the server or client 
            /// </summary>
            PayloadPacket = 3
        }

        public PacketTypes? PacketType { get; set; }

        /// <summary>
        /// The unique identifier of this packet.
        /// </summary>
        public uint Id { get; set; }

        /// <summary>
        /// the unique identifier of the client that sent this packet.
        /// </summary>
        public uint ClientId { get; set; }

        /// <summary>
        /// If this is a payload packet, what type of payload is it?
        /// </summary>
        public PayloadTypes? PayloadType { get; set; }

        /// <summary>
        /// If this is a payload packet, this is the payload
        /// </summary>
        public string? Payload { get; set; }

        /// <summary>
        /// Gets or sets the checksum value used to verify the integrity of data in the packet.
        /// </summary>
        public uint Checksum { get; set; }
    }
}

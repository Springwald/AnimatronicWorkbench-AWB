// Communicate between different devices on dotnet or arduino via COM port or Wifi
// https://github.com/Springwald/PacketLogistics
//
// (C) 2023 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using PacketLogistics.ComPorts.ComportPackets;
using PacketLogistics.Tools;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("PacketLogisticsTests")]
namespace PacketLogistics.ComPorts.Serialization
{
    internal class DataPacketSerializer
    {
        private readonly IComPortCommandConfig _comPortCommandConfig;

        public DataPacketSerializer(IComPortCommandConfig comPortCommandConfig)
        {
            _comPortCommandConfig = comPortCommandConfig;
        }

        public DataPacket? ByteArray2DataPacket(byte[] value, out string? errorMsg)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (value.Length == 0) throw new ArgumentOutOfRangeException(nameof(value));

            int pos = 0;

            // packet type
            var packetType = ByteArrayConverter.GetNextBytes(value, 1, ref pos)?.FirstOrDefault();
            if (packetType != (byte)PacketBase.PacketTypes.DataPacket)
            {
                errorMsg = $"Packet type is '{packetType}' not '{(byte)PacketBase.PacketTypes.DataPacket}'";
                return null;
            }

            // client id
            var senderIdBytes = ByteArrayConverter.GetNextBytes(value, 4, ref pos);
            if (senderIdBytes == null)
            {
                errorMsg = "Sender Id not found";
                return null;
            }
            var senderIdFromPacket = ByteArrayConverter.UintFrom4Bytes(senderIdBytes);

            // packet id
            var packetIdBytes = ByteArrayConverter.GetNextBytes(value, 4, ref pos);
            if (packetIdBytes == null)
            {
                errorMsg = "PacketId not found";
                return null;
            }
            var paketId = ByteArrayConverter.UintFrom4Bytes(packetIdBytes);
            
            var dataPacket = new DataPacket(id: paketId)
            {
                SenderId = senderIdFromPacket,
                Payload = ByteArrayConverter.GetNextBytes(value, value.Length - pos - 1, ref pos)
            };

            // checksum
            var checksum = ByteArrayConverter.GetNextBytes(value, 1, ref pos)?.FirstOrDefault();
            if (checksum == null)
            {
                errorMsg = "Checksum not found";
                return null;
            }

            // check checksum
            var expectedChecksum = ChecksumCalculator.Calculate(dataPacket);
            if (checksum != expectedChecksum)
            {
                errorMsg = $"Checksum is '{checksum}' not '{expectedChecksum}'";
                return null;
            }

            errorMsg = null;
            return dataPacket;
        }

        public byte[] DataPacket2ByteArray(DataPacket packet, uint senderId)
        {
            if (packet == null) throw new ArgumentNullException(nameof(packet));
            if (packet.Payload == null) throw new ArgumentNullException(nameof(packet.Payload));
            if (packet.Payload.Contains(_comPortCommandConfig.PacketHeaderBytes)) throw new ArgumentOutOfRangeException($"{nameof(packet.Payload)} contains packet-header!");
            if (senderId < 1) throw new ArgumentOutOfRangeException(paramName: nameof(senderId), message: "senderId must be >= 0 but is " + senderId);

            var checksum = ChecksumCalculator.Calculate(packet);

            var packetString = new List<byte>();
            packetString.Add((byte)PacketBase.PacketTypes.DataPacket);          // 1 byte
            packetString.AddRange(ByteArrayConverter.UintTo4Bytes(senderId));   // 4 bytes
            packetString.AddRange(ByteArrayConverter.UintTo4Bytes(packet.Id));  // 4 bytes
            packetString.AddRange(packet.Payload);                              // ...
            packetString.Add(checksum);                                         // 1 byte
            return packetString.ToArray();
        }
    }
}

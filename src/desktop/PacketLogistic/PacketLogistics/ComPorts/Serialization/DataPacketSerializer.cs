// Communicate between different devices on dotnet or arduino via COM port or Wifi
// https://github.com/Springwald/PacketLogistics
//
// (C) 2024 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using PacketLogistics.ComPorts.ComportPackets;
using PacketLogistics.Tools;
using System.Diagnostics;
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
            var senderIdBytes = ByteArrayConverter.GetNextBytes(value, 8, ref pos)?.ToArray();
            if (senderIdBytes == null)
            {
                errorMsg = "Sender Id not found";
                return null;
            }
            var senderIdFromPacket = ByteArrayConverter.UintFrom4Bytes(ByteArrayConverter.UnSplitBytes(senderIdBytes).ToArray());

            // packet id
            var packetIdBytes = ByteArrayConverter.GetNextBytes(value, 8, ref pos);
            if (packetIdBytes == null)
            {
                errorMsg = "PacketId not found";
                return null;
            }
            var paketId = ByteArrayConverter.UintFrom4Bytes(ByteArrayConverter.UnSplitBytes(packetIdBytes).ToArray());


            // payload length
            var payloadLengthBytes = ByteArrayConverter.GetNextBytes(value, 8, ref pos);
            if (payloadLengthBytes == null)
            {
                errorMsg = "Payload length not found";
                return null;
            }
            var payloadLength = ByteArrayConverter.UintFrom4Bytes(ByteArrayConverter.UnSplitBytes(packetIdBytes).ToArray());

            var dataPacket = new DataPacket(id: paketId)
            {
                SenderId = senderIdFromPacket,
                Payload = ByteArrayConverter.GetNextBytes(value, (int)payloadLength, ref pos)
            };

            // checksum
            var checksumBytesRaw = ByteArrayConverter.GetNextBytes(value, 2, ref pos);
            if (checksumBytesRaw == null)
            {
                errorMsg = "Checksum not found";
                return null;
            }
            var checksum = ByteArrayConverter.UnSplitBytes(checksumBytesRaw).FirstOrDefault();

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
            foreach(var b in _comPortCommandConfig.CommandBytes)
                if (packet.Payload.Contains(b)) throw new ArgumentOutOfRangeException($"{nameof(packet.Payload)} contains command byte {b}!");
            if (senderId < 1) throw new ArgumentOutOfRangeException(paramName: nameof(senderId), message: "senderId must be >= 0 but is " + senderId);

            var checksum = ChecksumCalculator.Calculate(packet);

           // Debug.WriteLine($"DataPacket2ByteArray: PacketId: {packet.Id}; SenderId: {senderId}; Checksum: {checksum}; PayloadLength: {packet.Payload.Length}");

            uint payloadLength = (uint)packet.Payload.Length;

            var packetBytes = new List<byte>();
            packetBytes.Add((byte)PacketBase.PacketTypes.DataPacket);          // 1 byte
            packetBytes.AddRange(ByteArrayConverter.SplitBytes(ByteArrayConverter.UintTo4Bytes(senderId)));   // 4*2 bytes
            packetBytes.AddRange(ByteArrayConverter.SplitBytes(ByteArrayConverter.UintTo4Bytes(packet.Id)));  // 4*2 bytes
            packetBytes.AddRange(ByteArrayConverter.SplitBytes(ByteArrayConverter.UintTo4Bytes(payloadLength)));  // 4*2 bytes
            packetBytes.AddRange(packet.Payload);                              // ...
            packetBytes.AddRange(ByteArrayConverter.SplitByte(checksum));      // 1*2 byte
            return packetBytes.ToArray();
        }
    }
}

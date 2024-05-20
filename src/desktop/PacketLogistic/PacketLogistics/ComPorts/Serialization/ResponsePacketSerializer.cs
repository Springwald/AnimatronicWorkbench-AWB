// Communicate between different devices on dotnet or arduino via COM port or Wifi
// https://github.com/Springwald/PacketLogistics
//
// (C) 2024 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using PacketLogistics.ComPorts.ComportPackets;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("PacketLogisticsTests")]
namespace PacketLogistics.ComPorts.Serialization
{
    internal class ResponsePacketSerializer
    {
        private readonly IComPortCommandConfig _comPortCommandConfig;

        public ResponsePacketSerializer(IComPortCommandConfig comPortCommandConfig)
        {
            _comPortCommandConfig = comPortCommandConfig;
        }

        public ResponsePacket? ByteArray2PacketResponse(byte[] value, out string? errorMsg)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (value.Length == 0) throw new ArgumentOutOfRangeException(nameof(value));


            // packet type
            var packetType = value[0];
            if (packetType != (byte)PacketBase.PacketTypes.ResponsePacket)
            {
                errorMsg = $"Packet type is '{packetType}' not '{(byte)PacketBase.PacketTypes.ResponsePacket}'";
                return null;
            }

            // original packet id
            int pos = 1;
            var originalPacketIdBytesRaw = ByteArrayConverter.GetNextBytes(value, 8, ref pos); // 4*2 bytes
            if (originalPacketIdBytesRaw == null)
            {
                errorMsg = "OriginalPacketId not found";
                return null;
            }
            var originalPacketIdBytes = ByteArrayConverter.UnSplitBytes(originalPacketIdBytesRaw).ToArray();
            var id = ByteArrayConverter.UintFrom4Bytes(originalPacketIdBytes);
            var packetResponse = new ResponsePacket(id: id);

            // ok
            var ok = ByteArrayConverter.GetNextBytes(value, 1, ref pos)?.FirstOrDefault();
            if (ok == null)
            {
                errorMsg = $"ok not found";
                return null;
            }
            packetResponse.Ok = ok == 1;

            // message
            var message = ByteArrayConverter.GetNextBytes(value, value.Length - pos - 2, ref pos);
            packetResponse.Message = ByteArrayConverter.BytesToAsciiString(message ?? Array.Empty<byte>());

            // checksum
            var checksumRawBytes = ByteArrayConverter.GetNextBytes(value, 2, ref pos);
            var checksum = ByteArrayConverter.UnSplitBytes(checksumRawBytes ?? [])?.FirstOrDefault();
            if (checksum == null)
            {
                errorMsg = "Checksum not found";
                return null;
            }

            var expectedChecksum = ChecksumCalculator.Calculate(packetResponse);

            // check checksum
            if (checksum != expectedChecksum)
            {
                errorMsg = $"Checksum is '{checksum}' not '{expectedChecksum}', message: '{packetResponse.Message}'";
                return null;
            }

            errorMsg = null;
            return packetResponse;
        }

        public byte[] PacketResponse2ByteArray(ResponsePacket packetResponse)
        {
            if (packetResponse == null) throw new ArgumentNullException(nameof(packetResponse));

            var checkSum = ChecksumCalculator.Calculate(packetResponse);

            var packetBytes = new List<byte>();
            packetBytes.Add((byte)PacketBase.PacketTypes.ResponsePacket);
            packetBytes.AddRange(ByteArrayConverter.SplitBytes(ByteArrayConverter.UintTo4Bytes(packetResponse.Id)));
            packetBytes.Add(packetResponse.Ok ? (byte)1 : (byte)0);
            packetBytes.AddRange(ByteArrayConverter.AsciiStringToBytes(packetResponse.Message ?? string.Empty));
            packetBytes.AddRange(ByteArrayConverter.SplitByte(checkSum));
            return packetBytes.ToArray();
        }
    }
}

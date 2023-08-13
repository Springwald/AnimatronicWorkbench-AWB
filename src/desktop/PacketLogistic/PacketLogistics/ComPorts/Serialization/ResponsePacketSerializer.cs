// Communicate between different devices on dotnet or arduino via COM port or Wifi
// https://github.com/Springwald/PacketLogistics
//
// (C) 2023 Daniel Springwald, Bochum Germany
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

            int pos = 0;

            // packet type
            var packetType = ByteArrayConverter.GetNextBytes(value, 1, ref pos)?.FirstOrDefault();
            if (packetType != (byte)PacketBase.PacketTypes.ResponsePacket)
            {
                errorMsg = $"Packet type is '{packetType}' not '{(byte)PacketBase.PacketTypes.ResponsePacket}'";
                return null;
            }

            // original packet id
            var originalPacketIdBytes = ByteArrayConverter.GetNextBytes(value, 4, ref pos);
            if (originalPacketIdBytes == null)
            {
                errorMsg = "OriginalPacketId not found";
                return null;
            }

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
            var message = ByteArrayConverter.GetNextBytes(value, value.Length - pos - 1, ref pos);
            packetResponse.Message = ByteArrayConverter.BytesToAsciiString(message ?? Array.Empty<byte>());

            // checksum
            var checksum = ByteArrayConverter.GetNextBytes(value, 1, ref pos)?.FirstOrDefault();
            if (checksum == null)
            {
                errorMsg = "Checksum not found";
                return null;
            }

            // packet end byte
            //var endByte = ByteArrayConverter.GetNextBytes(value, 1, ref pos)?.FirstOrDefault();
            //if (endByte != _comPortCommandConfig.PacketStartEndByte)
            //{
            //    errorMsg = $"Packet end not {_comPortCommandConfig.PacketStartEndByte} but {endByte}";
            //    return null;
            //}

            var expectedChecksum = ChecksumCalculator.Calculate(packetResponse);

            // check checksum
            if (checksum != expectedChecksum)
            {
                errorMsg = $"Checksum is '{checksum}' not '{expectedChecksum}'";
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
            packetBytes.AddRange(ByteArrayConverter.UintTo4Bytes(packetResponse.Id));
            packetBytes.Add(packetResponse.Ok ? (byte)1 : (byte)0);
            packetBytes.AddRange(ByteArrayConverter.AsciiStringToBytes(packetResponse.Message ?? string.Empty));
            packetBytes.Add(checkSum);
            return packetBytes.ToArray();
        }
    }
}

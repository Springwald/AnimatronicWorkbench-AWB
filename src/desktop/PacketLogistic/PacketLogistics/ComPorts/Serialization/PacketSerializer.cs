﻿// Communicate between different devices on dotnet or arduino via COM port or Wifi
// https://github.com/Springwald/PacketLogistics
//
// (C) 2023 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using PacketLogistics.ComPorts.ComportPackets;
using static PacketLogistics.ComPorts.ComportPackets.PacketBase;

namespace PacketLogistics.ComPorts.Serialization
{
    internal class PacketSerializer
    {
        private readonly IComPortCommandConfig _comPortCommandConfig;

        public PacketSerializer(IComPortCommandConfig comPortCommandConfig)
        {
            _comPortCommandConfig = comPortCommandConfig;
        }

        public PacketBase? GetPacketFromByteArray(byte[] value, out string? errorMsg)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (value.Length == 0) throw new ArgumentOutOfRangeException(nameof(value));

            // packet type
            var packetTypeByte = value[0];

            int pos = 1;
            switch (packetTypeByte)
            {
                case (byte)PacketTypes.AlifePacket:
                    // client id
                    var clientIdBytesRaw = ByteArrayConverter.GetNextBytes(value, 8, ref pos);
                    if (clientIdBytesRaw == null)
                    {
                        errorMsg = $"No client Id in alife packet?!?";
                        return null;
                    }
                    var clientIdBytes = ByteArrayConverter.UnSplitBytes(clientIdBytesRaw).ToArray();
                    var clientId = ByteArrayConverter.UintFrom4Bytes(clientIdBytes);
                    errorMsg = null;
                    return new AlifePacket(clientId);

                case (byte)PacketTypes.DataPacket:
                    return new DataPacketSerializer(_comPortCommandConfig)
                        .ByteArray2DataPacket(value, out errorMsg);

                case (byte)PacketTypes.ResponsePacket:
                    return new ResponsePacketSerializer(_comPortCommandConfig)
                        .ByteArray2PacketResponse(value, out errorMsg);

                default:
                    errorMsg = $"Unknown packet type byte '{packetTypeByte}'";
                    return null;
            }
        }
    }
}

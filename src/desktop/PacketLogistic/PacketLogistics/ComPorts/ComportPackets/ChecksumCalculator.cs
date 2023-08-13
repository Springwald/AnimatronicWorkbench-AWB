// Communicate between different devices on dotnet or arduino via COM port or Wifi
// https://github.com/Springwald/PacketLogistics
//
// (C) 2023 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("PacketLogisticsTests")]
namespace PacketLogistics.ComPorts.ComportPackets
{
    internal static class ChecksumCalculator
    {
        public static byte Calculate(DataPacket packet)
        {
            byte result = 0;
            if (packet == null) throw new ArgumentNullException(nameof(packet));

            foreach (byte b in packet.Payload ?? Array.Empty<byte>()) result += b;
            foreach (byte b in ByteArrayConverter.UintTo4Bytes(packet.Id)) result += b;

            return result;
        }

        public static byte Calculate(ResponsePacket packetResponse)
        {
            byte result = 0;
            if (packetResponse == null) throw new ArgumentNullException(nameof(packetResponse));

            foreach (byte b in ByteArrayConverter.AsciiStringToBytes(packetResponse.Message ?? string.Empty)) result += b;
            foreach (byte b in ByteArrayConverter.UintTo4Bytes(packetResponse.Id)) result += b;
            result += packetResponse.Ok ? (byte)1 : (byte)0;

            return result;
        }
    }
}

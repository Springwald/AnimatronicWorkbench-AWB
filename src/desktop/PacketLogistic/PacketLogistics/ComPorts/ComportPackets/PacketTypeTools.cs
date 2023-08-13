//// Communicate between different devices on dotnet or arduino via COM port or Wifi
//// https://github.com/Springwald/PacketLogistics
////
//// (C) 2023 Daniel Springwald, Bochum Germany
//// Springwald Software  -   www.springwald.de
//// daniel@springwald.de -  +49 234 298 788 46
//// All rights reserved
//// Licensed under MIT License

//using PacketLogistics.ComPorts;
//using PacketLogistics.ComPorts.Serialization;

//namespace PacketLogistics.ComPorts.ComportPackets
//{
//    internal class PacketTypeTools
//    {
//        private readonly IComPortCommandConfig _comPortCommandConfig;

//        public PacketTypeTools(IComPortCommandConfig comPortCommandConfig, uint clientId)
//        {
//            _comPortCommandConfig = comPortCommandConfig;
//        }

//        public PacketBase? GetPacketTypeFromByteArray(byte[] value, out string? errorMsg)
//        {
//            if (value == null) throw new ArgumentNullException(nameof(value));
//            if (value.Length == 0) throw new ArgumentOutOfRangeException(nameof(value));

//            int pos = 0;

//            // Packet start byte
//            var startByte = ByteArrayConverter.GetNextBytes(value, 1, ref pos)?.FirstOrDefault();
//            if (startByte != _comPortCommandConfig.PacketStartEndByte)
//            {
//                errorMsg = $"Packet start not {_comPortCommandConfig.PacketStartEndByte} but {startByte}";
//                return null;
//            }

//            // Packet header
//            var header = ByteArrayConverter.GetNextBytes(value, 3, ref pos);
//            if (!ByteArrayConverter.AreEqual(_comPortCommandConfig.PacketHeaderBytes, header))
//            {
//                errorMsg = $"Packet header is '{ByteArrayConverter.BytesToAsciiString(header ?? Array.Empty<byte>())}' not '{_comPortCommandConfig.PacketHeader}'";
//                return null;
//            }

//            // packet type
//            var packetTypeByte = ByteArrayConverter.GetNextBytes(value, 1, ref pos)?.FirstOrDefault();
//            if (packetTypeByte == null)
//            {
//                errorMsg = $"No Packet Type received!";
//                return null;
//            }

//            switch (packetTypeByte)
//            {
//                case (byte)PacketBase.PacketTypes.DataPacket:
//                    var dataPacket = new DataPacketSerializer(_comPortCommandConfig).ByteArray2DataPacket(value, _clientId, out errorMsg);
//                    return PacketBase.PacketTypes.DataPacket;

//                case (byte)PacketBase.PacketTypes.ResponsePacket:
//                    errorMsg = null;
//                    return PacketTypes.ResponsePacket;

//                default:
//                    errorMsg = $"Unknown packet type byte '{packetTypeByte}'";
//                    return null;
//            }
//        }
//    }
//}

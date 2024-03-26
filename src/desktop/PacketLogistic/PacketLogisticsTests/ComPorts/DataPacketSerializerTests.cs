// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PacketLogistics.ComPorts.ComportPackets;
using PacketLogistics.ComPorts.Serialization;

namespace PacketLogistics.ComPorts.Tests
{
    [TestClass()]
    public class DataPacketSerializerTests
    {
        [TestMethod()]
        public void DataPacketSerializerRoundTripTest()
        {
            var config = new ComPortCommandConfig(packetHeader: "XYZ");
            uint clientId = 345;
            var serializer = new DataPacketSerializer(config);

            var testPacket = new DataPacket(id: 1234567890)
            {
                Payload = ByteArrayConverter.AsciiStringToBytes("Hello World!")
            };

            var testPacketBytes = serializer.DataPacket2ByteArray(testPacket, senderId: clientId);

            var reversedTestPacket = serializer.ByteArray2DataPacket(testPacketBytes, out string? errorMsg);

            Assert.AreEqual(null, errorMsg);
            Assert.IsNotNull(reversedTestPacket);
            Assert.AreEqual(testPacket.Id, reversedTestPacket.Id);
            Assert.AreEqual(testPacket.Payload.Length, reversedTestPacket.Payload.Length);
            for (int i = 0; i < testPacket.Payload.Length; i++)
            {
                Assert.AreEqual(testPacket.Payload[i], reversedTestPacket.Payload[i]);
            }
        }

        [TestMethod()]
        public void ByteArray2DataPacketTest()
        {
            var config = new ComPortCommandConfig(packetHeader: "XYZ");
            uint clientId = 129;
            var serializer = new DataPacketSerializer(config);

            var testPacket = new DataPacket(id: 1234567890)
            {
                Payload = ByteArrayConverter.AsciiStringToBytes("Hello World!")
            };

            byte[] packetIdBytes = ByteArrayConverter.UintTo4Bytes(testPacket.Id);
            byte[] packetIdBytesSplit = ByteArrayConverter.SplitBytes(packetIdBytes).ToArray();

            var checksum = ChecksumCalculator.Calculate(testPacket);
            var checksumSplit = ByteArrayConverter.SplitByte(checksum).ToArray();

            var testPacketBytes = new byte[]
            {
                (byte)PacketBase.PacketTypes.DataPacket,        // packet type
                0,0,0,0,0,0,129,0,                                // client id
                packetIdBytesSplit[0],packetIdBytesSplit[1],packetIdBytesSplit[2],packetIdBytesSplit[3],packetIdBytesSplit[4],packetIdBytesSplit[5],packetIdBytesSplit[6],packetIdBytesSplit[7], // packet id
                (byte)'H', (byte)'e', (byte)'l', (byte)'l', (byte)'o', (byte)' ', (byte)'W', (byte)'o', (byte)'r', (byte)'l', (byte)'d', (byte)'!',
                checksumSplit[0], checksumSplit[1]              // checksum
            };

            var reversedTestPacket = serializer.ByteArray2DataPacket(testPacketBytes, out string? errorMsg);

            Assert.AreEqual(null, errorMsg);
            Assert.IsNotNull(reversedTestPacket);
            Assert.AreEqual(testPacket.Id, reversedTestPacket.Id, nameof(testPacket.Id));
            Assert.AreEqual(testPacket.Payload?.Length, reversedTestPacket.Payload?.Length ?? 0, nameof(testPacket.Payload.Length));
            for (int i = 0; i < testPacket.Payload?.Length; i++)
            {
                Assert.AreEqual(testPacket.Payload[i], reversedTestPacket?.Payload[i], "payload pos " + i);
            }
        }

        [TestMethod()]
        public void DataPacket2ByteArrayTest()
        {
            uint testClientId = 234;

            var config = new ComPortCommandConfig(packetHeader: "XYZ");
            var serializer = new DataPacketSerializer(config);

            var testPacket = new DataPacket(id: 1234567890)
            {
                Payload = ByteArrayConverter.AsciiStringToBytes("Hello World!")
            };

            var checksum = ChecksumCalculator.Calculate(testPacket);

            var testPacketBytes = serializer.DataPacket2ByteArray(testPacket, testClientId);

            var expectedBytes = new List<byte>();
            expectedBytes.Add((byte)PacketBase.PacketTypes.DataPacket);
            expectedBytes.AddRange(ByteArrayConverter.SplitBytes(ByteArrayConverter.UintTo4Bytes(testClientId)));
            expectedBytes.AddRange(ByteArrayConverter.SplitBytes(ByteArrayConverter.UintTo4Bytes(testPacket.Id)));
            expectedBytes.AddRange(testPacket.Payload);
            expectedBytes.AddRange(ByteArrayConverter.SplitByte(checksum));

            Assert.AreEqual(expectedBytes.Count, testPacketBytes.Length, nameof(expectedBytes.Count));
            for (int i = 0; i < expectedBytes.Count; i++)
            {
                Assert.AreEqual(expectedBytes[i], testPacketBytes[i]);
            }
        }
    }
}
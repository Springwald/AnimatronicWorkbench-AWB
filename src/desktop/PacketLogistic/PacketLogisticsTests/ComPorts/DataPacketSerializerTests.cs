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

            var checksum = ChecksumCalculator.Calculate(testPacket);

            var testPacketBytes = new byte[]
            {
                (byte)PacketBase.PacketTypes.DataPacket,        // packet type
                0,0,0,129,                                      // client id
                packetIdBytes[0],packetIdBytes[1],packetIdBytes[2],packetIdBytes[3], // packet id
                (byte)'H', (byte)'e', (byte)'l', (byte)'l', (byte)'o', (byte)' ', (byte)'W', (byte)'o', (byte)'r', (byte)'l', (byte)'d', (byte)'!',
                checksum,                                       // checksum
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

            var config = new ComPortCommandConfig( packetHeader: "XYZ");
            var serializer = new DataPacketSerializer(config);

            var testPacket = new DataPacket(id: 1234567890)
            {
                Payload = ByteArrayConverter.AsciiStringToBytes("Hello World!")
            };

            var checksum = ChecksumCalculator.Calculate(testPacket);

            var testPacketBytes = serializer.DataPacket2ByteArray(testPacket, testClientId);

            var expectedBytes = new List<byte>();
            expectedBytes.Add((byte)PacketBase.PacketTypes.DataPacket);
            expectedBytes.AddRange(ByteArrayConverter.UintTo4Bytes(testClientId));
            expectedBytes.AddRange(ByteArrayConverter.UintTo4Bytes(testPacket.Id));
            expectedBytes.AddRange(testPacket.Payload);
            expectedBytes.Add(checksum);

            Assert.AreEqual(expectedBytes.Count, testPacketBytes.Length, nameof(expectedBytes.Count));
            for (int i = 0; i < expectedBytes.Count; i++)
            {
                Assert.AreEqual(expectedBytes[i], testPacketBytes[i]);
            }
        }
    }
}
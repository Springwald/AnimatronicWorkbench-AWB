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
    public class PacketResponseSerializerTests
    {
        [TestMethod()]
        public void PacketResponseSerializerRoundTripTest()
        {
            var config = new ComPortCommandConfig(packetHeader: "XYZ");
            var serializer = new ResponsePacketSerializer(config);

            var testPacketResponse = new ResponsePacket(id: 123)
            {
                Ok = true,
                Message = "Hello World!"
            };

            var testPacketResponseBytes = serializer.PacketResponse2ByteArray(testPacketResponse);
            var reversedTestPacketResponse = serializer.ByteArray2PacketResponse(testPacketResponseBytes, out string? errorMsg);

            Assert.AreEqual(null, errorMsg);
            Assert.IsNotNull(reversedTestPacketResponse);
            Assert.AreEqual(testPacketResponse.Id, reversedTestPacketResponse.Id);
            Assert.AreEqual(testPacketResponse.Ok, reversedTestPacketResponse.Ok);
            Assert.AreEqual(testPacketResponse.Message, reversedTestPacketResponse.Message);
        }

        [TestMethod()]
        public void ByteArray2PacketResponseTest()
        {
            var config = new ComPortCommandConfig(packetHeader: "XYZ");
            var serializer = new ResponsePacketSerializer(config);

            var testPacketResponse = new ResponsePacket(id: 123)
            {
                Ok = true,
                Message = "Hello World!"
            };

            var checkSum = ChecksumCalculator.Calculate(testPacketResponse);
            var checkSumSplit = ByteArrayConverter.SplitByte(checkSum).ToArray();

            var testPacketResponseBytes = new byte[]
            {
                (byte)PacketBase.PacketTypes.ResponsePacket,    // packet type
                0, 0, 0, 0, 0, 0, 123, 0,                          // original packet id
                (byte)(testPacketResponse.Ok ? 1: 0),           // ok
                (byte)'H', (byte)'e', (byte)'l', (byte)'l', (byte)'o', (byte)' ', (byte)'W', (byte)'o', (byte)'r', (byte)'l', (byte)'d', (byte)'!',
                checkSumSplit[0], checkSumSplit[1]                               // checksum
            };

            var testPacketResponseAsBytes = serializer.ByteArray2PacketResponse(testPacketResponseBytes, out string? errorMsg);

            Assert.AreEqual(null, errorMsg);
            Assert.IsNotNull(testPacketResponseAsBytes);
            Assert.AreEqual(testPacketResponseAsBytes.Id, testPacketResponse.Id);
            Assert.AreEqual(testPacketResponseAsBytes.Ok, testPacketResponse.Ok);
            Assert.AreEqual(testPacketResponseAsBytes.Message, testPacketResponse.Message);
        }

        [TestMethod()]
        public void PacketResponse2ByteArrayTest()
        {
            var config = new ComPortCommandConfig(packetHeader: "XYZ");
            var serializer = new ResponsePacketSerializer(config);

            var testPacketResponse = new ResponsePacket(id: 123)
            { Message = "Hello World!", Ok = true };

            var checksum = ChecksumCalculator.Calculate(testPacketResponse);

            var testPacketResponseBytes = serializer.PacketResponse2ByteArray(testPacketResponse);

            var expectedBytes = new List<byte>();
            expectedBytes.Add((byte)PacketBase.PacketTypes.ResponsePacket);
            expectedBytes.AddRange(ByteArrayConverter.UintTo4Bytes(testPacketResponse.Id));
            expectedBytes.Add(testPacketResponse.Ok ? (byte)1 : (byte)0);
            expectedBytes.AddRange(ByteArrayConverter.AsciiStringToBytes(testPacketResponse.Message ?? string.Empty));
            expectedBytes.Add(checksum);

            Assert.AreEqual(testPacketResponseBytes.Length, expectedBytes.Count);
            for (int i = 0; i < expectedBytes.Count; i++)
            {
                Assert.AreEqual(expectedBytes[i], testPacketResponseBytes[i]);
            }
        }
    }
}
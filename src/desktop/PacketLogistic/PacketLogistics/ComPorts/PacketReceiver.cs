// Communicate between different devices on dotnet or arduino via COM port or Wifi
// https://github.com/Springwald/PacketLogistics
//
// (C) 2023 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using PacketLogistics.ComPorts.ComportPackets;
using PacketLogistics.ComPorts.Serialization;
using PacketLogistics.Tools;

namespace PacketLogistics.ComPorts
{
    internal class PacketReceiver
    {
        public delegate void PacketReceivedDelegate(PacketBase packet);
        public delegate void ErrorDelegate(string errorMessage);

        private readonly Esp32SerialPort _esp32SerialPort;
        private readonly IComPortCommandConfig _comPortCommandConfig;
        private readonly PacketReceivedDelegate _packetReceivedDelegate;
        private readonly ErrorDelegate _errorDelegate;
        private readonly PacketSerializer _packetSerializer;
        private readonly List<byte> _receiveBuffer = new();

        public PacketReceiver(
            Esp32SerialPort esp32SerialPort,
            IComPortCommandConfig comPortCommandConfig,
            uint clientId,
            PacketReceivedDelegate packetReceivedDelegate,
            ErrorDelegate errorDelegate)
        {
            _esp32SerialPort = esp32SerialPort;
            _comPortCommandConfig = comPortCommandConfig;
            _packetReceivedDelegate = packetReceivedDelegate;
            _errorDelegate = errorDelegate;
            _packetSerializer = new PacketSerializer(_comPortCommandConfig);
        }

        public async Task<PacketBase?> TryReceivePacket()
        {
            PacketBase? packet = null;

            int bytesToRead = _esp32SerialPort.BytesToRead;

            if (bytesToRead == 0)
            {
                await Task.Delay(1);
            }
            else
            {
                while (_esp32SerialPort.BytesToRead > 0)
                {
                    byte chr = (byte)_esp32SerialPort.ReadByte();
                    _receiveBuffer.Add(chr);

                    if (_receiveBuffer.EndsWith(_comPortCommandConfig.PacketHeaderBytes))
                    {
                        var packetContent = _receiveBuffer.Take(_receiveBuffer.Count - _comPortCommandConfig.PacketHeaderBytes.Length).ToArray();
                        _receiveBuffer.Clear();

                        if (packetContent.Length == 0)
                        {
                        }
                        else
                        {
                            packet = _packetSerializer.GetPacketFromByteArray(packetContent, out string? errMsg);
                            if (packet == null)
                            {
                                _errorDelegate(errMsg ?? $"Unknown error while deserializing receive buffer '{_receiveBuffer.ToString()}'");
                            }
                            else
                            {
                                _packetReceivedDelegate(packet);
                            }
                        }
                    }
                }
            }

            return packet;

        }
    }
}

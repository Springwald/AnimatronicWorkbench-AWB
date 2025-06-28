// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using PacketLogistics.PacketPayloadWrapper;
using PacketLogistics.Tools;

namespace PacketLogistics.ComPorts
{
    internal class PacketReceiver<PayloadTypes> where PayloadTypes : Enum
    {

        public delegate void PacketReceivedDelegate(PacketEnvelope<PayloadTypes> packet);
        public delegate void ErrorDelegate(string errorMessage);

        private readonly Esp32SerialPort _esp32SerialPort;
        private readonly IComPortCommandConfig _comPortCommandConfig;
        private readonly PacketReceivedDelegate _packetReceivedDelegate;
        private readonly ErrorDelegate _errorDelegate;

        private readonly List<byte> _receiveBuffer = new();
        private bool _insidePacketStream = false;


        public PacketReceiver(
            Esp32SerialPort esp32SerialPort,
            IComPortCommandConfig comPortCommandConfig,
            PacketReceivedDelegate packetReceivedDelegate,
            ErrorDelegate errorDelegate)
        {
            _esp32SerialPort = esp32SerialPort;
            _comPortCommandConfig = comPortCommandConfig;
            _packetReceivedDelegate = packetReceivedDelegate;
            _errorDelegate = errorDelegate;
        }

        public async Task<PacketEnvelope<PayloadTypes>?> TryReceivePacket()
        {
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

                    if (_receiveBuffer.EndsWith(_comPortCommandConfig.PacketHeaderAsBytes))// the start of a packet stream is received
                    {
                        _receiveBuffer.Clear();
                        _insidePacketStream = true;
                        continue;
                    }

                    if (_receiveBuffer.EndsWith(_comPortCommandConfig.PacketFooterAsBytes)) // the end of a packet stream is received
                    {
                        if (_insidePacketStream == false)
                        {
                            // we received a packet footer without a start packet header
                            _errorDelegate("Received packet footer without a start packet header!");
                            _receiveBuffer.Clear();
                            _insidePacketStream = false;
                            continue;
                        }

                        var packetContent = _receiveBuffer.Take(_receiveBuffer.Count - _comPortCommandConfig.PacketFooterAsBytes.Length).ToArray();

                        _receiveBuffer.Clear();
                        _insidePacketStream = false;

                        if (packetContent.Length == 0)
                        {
                            // Reveived an empty packet?!? ignore it
                            _errorDelegate("Received an empty packet (only packet header and footer)!");
                            continue;
                        }

                        // deserialize the packet content
                        var packetWrapper = new PacketWrapper<PayloadTypes>();
                        var packetContentAsString = _comPortCommandConfig.Encoding.GetString(packetContent);
                        var packetUnwrapResult = packetWrapper.UnwrapPacket(packetContentAsString);

                        if (packetUnwrapResult == null)
                        {
                            // we could not deserialize the packet content
                            var errMsg = $"Could not deserialize packet content '{packetContentAsString}'";
                            _errorDelegate(errMsg);
                            continue;
                        }

                        if (packetUnwrapResult.Ok == false)
                        {
                            // we could not deserialize the packet content
                            var errMsg = $"Could not deserialize packet content '{packetContentAsString}' with error '{packetUnwrapResult.ErrorMessage}'";
                            _errorDelegate(errMsg);
                            continue;
                        }

                        var packetEnvelope = packetUnwrapResult.Packet;
                        if (packetEnvelope == null)
                        {
                            var errMsg = $"PacketEnvelope is null after deserializing packet content '{packetContentAsString}'";
                            _errorDelegate(errMsg);
                            continue;
                        }

                        _packetReceivedDelegate(packetEnvelope);
                    }
                }
            }
            return null;
        }
    }
}

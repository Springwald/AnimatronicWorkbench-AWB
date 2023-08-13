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
using System.Collections.Concurrent;

namespace PacketLogistics.ComPorts
{
    public class PacketSenderReceiverComPort : PacketSenderReceiver
    {
        private Esp32SerialPort? _serialPort;
        private readonly string _serialPortName;
        private uint _actualPacketId;
        private readonly IComPortCommandConfig _comPortCommandConfig;
        private readonly DataPacketSerializer _packetSerializer;
        private ConcurrentBag<PacketBase> _receivedPackets = new();
        private PacketReceiver? _packetReceiver;

        public uint ClientId { get; }

        /// <param name="serialPortName">If the value is not set, the serial port is searched for automatically</param>
        public PacketSenderReceiverComPort(string serialPortName, uint clientID, IComPortCommandConfig comPortCommandConfig)
        {
            if (string.IsNullOrWhiteSpace(serialPortName)) throw new ArgumentNullException(nameof(serialPortName));
            _serialPortName = serialPortName;
            this.ClientId = clientID;
            _comPortCommandConfig = comPortCommandConfig;
            _packetSerializer = new DataPacketSerializer(_comPortCommandConfig);
        }

        public override void Dispose()
        {
            if (_packetReceiver != null)
            {
                _packetReceiver = null;
            }
            if (_serialPort != null)
            {
                if (_serialPort.IsOpen)
                    _serialPort.Close();
                _serialPort.Dispose();
                _serialPort = null;
            }
        }

        protected override async Task<bool> ConnectInternal()
        {
            _serialPort = new Esp32SerialPort(_serialPortName);
            try
            {
                _serialPort.Open();
                _packetReceiver = new PacketReceiver(esp32SerialPort: _serialPort,
                    comPortCommandConfig: _comPortCommandConfig,
                   clientId: this.ClientId,
                   packetReceivedDelegate: this.PacketReceiverPacketReceived,
                   errorDelegate: (string errMsg) => { base.Error(errMsg); });
                return true;
            }
            catch (Exception ex)
            {
                base.Error($"Error connecting to port '{_serialPortName}': {ex.Message}");
                await Task.Delay(100);
            }
            if (_serialPort?.IsOpen == true)
            {
                _serialPort.Close();
                _serialPort = null;
            }
            return false;
        }

        private void PacketReceiverPacketReceived(PacketBase packet)
        {
            switch (packet?.PacketType)
            {
                case PacketBase.PacketTypes.DataPacket:
                    throw new NotImplementedException();
                    break;

                case PacketBase.PacketTypes.ResponsePacket:
                    // handled in packet sending method
                    break;

                case PacketBase.PacketTypes.AlifePacket:
                    // only needed when connecting
                    break;

                default:
                    base.Error($"Received unknown packet type: {packet?.PacketType}");
                    break;

            }
        }

        protected override async Task<PacketSendResult> SendPacketInternal(byte[] payload)
        {
            if (_serialPort == null)
            {
                var errMsg = $"serial port {_serialPortName} == null ?!?";
                base.Error(errMsg);
                return new PacketSendResult
                {
                    OriginalPacketTimestampUtc = DateTime.UtcNow,
                    OriginalPacketId = null,
                    Ok = false,
                    Message = errMsg,
                };
            }

            if (_packetReceiver == null)
            {
                var errMsg = $"packet receiver for port {_serialPortName} == null ?!?";
                base.Error(errMsg);
                return new PacketSendResult
                {
                    OriginalPacketTimestampUtc = DateTime.UtcNow,
                    OriginalPacketId = null,
                    Ok = false,
                    Message = errMsg,
                };
            }

            var serialPort = _serialPort;

            if (serialPort.IsOpen == false)
            {
                var errMsg = $"serial port {serialPort.PortName} is not open ?!?";
                base.Error(errMsg);
                return new PacketSendResult
                {
                    OriginalPacketTimestampUtc = DateTime.UtcNow,
                    OriginalPacketId = null,
                    Ok = false,
                    Message = errMsg,
                };
            }

            _actualPacketId++;
            if (_actualPacketId >= uint.MaxValue) _actualPacketId = 0;

            var packet = new DataPacket(id: _actualPacketId)
            {
                Payload = payload,
                TimestampUtc = DateTime.UtcNow
            };

            // send packet
            var message = _packetSerializer.DataPacket2ByteArray(packet, this.ClientId);
            //serialPort.DiscardOutBuffer();
            serialPort.Write(_comPortCommandConfig.PacketHeaderBytes, 0, _comPortCommandConfig.PacketHeaderBytes.Length);
            serialPort.Write(message, 0, message.Length);
            serialPort.Write(_comPortCommandConfig.PacketHeaderBytes, 0, _comPortCommandConfig.PacketHeaderBytes.Length);

            var timeOutDateTime = DateTime.UtcNow + base._timeout;

            while (DateTime.UtcNow < timeOutDateTime)
            {
                var packetReceived = await _packetReceiver.TryReceivePacket();

                if (packetReceived?.PacketType == PacketBase.PacketTypes.ResponsePacket)
                {
                    var responsePacket = (ResponsePacket)packetReceived;
                    if (responsePacket.Id == packet.Id)
                    {
                        return new PacketSendResult
                        {
                            Ok = responsePacket.Ok,
                            OriginalPacketId = packet.Id,
                            OriginalPacketTimestampUtc = packet.TimestampUtc,
                            Message = responsePacket.Message,
                        };
                    }
                }
            }

            base.Error($"Timeout sending packet {packet.Id} to {_serialPortName}");

            return new PacketSendResult
            {
                Ok = false,
                OriginalPacketId = packet.Id,
                OriginalPacketTimestampUtc = packet.TimestampUtc,
                Message = "Timeout",
            };
        }
    }
}

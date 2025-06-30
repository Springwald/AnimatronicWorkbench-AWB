// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using PacketLogistics.ComPorts.ComportPackets;
using PacketLogistics.PacketPayloadWrapper;
using System.Diagnostics;

namespace PacketLogistics.ComPorts
{
    public class PacketSenderReceiverComPort<PayloadTypes> : PacketSenderReceiver<PayloadTypes> where PayloadTypes : Enum
    {
        private Esp32SerialPort? _serialPort;
        private readonly string _serialPortName;
        private uint _actualPacketId = 1;
        private readonly IComPortCommandConfig _comPortCommandConfig;
        private PacketReceiver<PayloadTypes>? _packetReceiver;

        public uint ClientId { get; }

        /// <param name="serialPortName">If the value is not set, the serial port is searched for automatically</param>
        public PacketSenderReceiverComPort(string serialPortName, uint clientID, IComPortCommandConfig comPortCommandConfig)
        {
            if (string.IsNullOrWhiteSpace(serialPortName)) throw new ArgumentNullException(nameof(serialPortName));
            _serialPortName = serialPortName;
            this.ClientId = clientID;
            _comPortCommandConfig = comPortCommandConfig;
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

        /// <summary>
        /// Connects to the serial port
        /// </summary>
        protected override async Task<bool> ConnectInternal()
        {
            _serialPort = new Esp32SerialPort(_serialPortName);
            try
            {
                _serialPort.Open();
                _packetReceiver = new PacketReceiver<PayloadTypes>(esp32SerialPort: _serialPort,
                    comPortCommandConfig: _comPortCommandConfig,
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

        private void PacketReceiverPacketReceived(PacketEnvelope<PayloadTypes> packet)
        {
            switch (packet?.PacketType)
            {
                case PacketEnvelope<PayloadTypes>.PacketTypes.PayloadPacket:
                    throw new NotImplementedException();

                case PacketEnvelope<PayloadTypes>.PacketTypes.ResponsePacket:
                    // handled in packet sending method
                    break;

                case PacketEnvelope<PayloadTypes>.PacketTypes.AlivePacket:
                    // only needed when connecting
                    break;

                default:
                    base.Error($"Received unknown packet type: {packet?.PacketType}");
                    break;

            }
        }


        protected override async Task<PacketSendResult> SendPacketInternal(PayloadTypes payloadType, string payload)
        {
            if (_serialPort == null)
            {
                var errMsg = $"serial port {_serialPortName} == null ?!?";
                base.Error(errMsg);
                return new PacketSendResult
                {
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
                    OriginalPacketId = null,
                    Ok = false,
                    Message = errMsg,
                };
            }

            _actualPacketId++;
            if (_actualPacketId >= uint.MaxValue) _actualPacketId = 0;

            var packetId = _actualPacketId;

            var envelopeWrapper = new PacketWrapper<PayloadTypes>();
            var packetEnvelope = envelopeWrapper.WrapDataPacket(
                packetId: packetId,
                packetType: PacketEnvelope<PayloadTypes>.PacketTypes.PayloadPacket,
                payLoadType: payloadType,
                payload: payload);

            var packetAsBytes = _comPortCommandConfig.Encoding.GetBytes(packetEnvelope);

            Debug.WriteLine("send: " + packetEnvelope);

            // Send the packetBytes to the serial port
            if (packetAsBytes == null || packetAsBytes.Length == 0)
            {
                var errMsg = $"Error serializing packet {packetId} to byte array!";
                base.Error(errMsg);
                return new PacketSendResult
                {
                    OriginalPacketId = packetId,
                    Ok = false,
                    Message = errMsg,
                };
            }

            if (packetAsBytes.IndexOf(_comPortCommandConfig.PacketHeaderAsBytes) != -1)
            {
                var errMsg = $"Packet {packetId} does contain packet header bytes!";
                base.Error(errMsg);
                return new PacketSendResult
                {
                    OriginalPacketId = packetId,
                    Ok = false,
                    Message = errMsg,
                };
            }

            if (packetAsBytes.IndexOf(_comPortCommandConfig.PacketFooterAsBytes) != -1)
            {
                var errMsg = $"Packet {packetId} does contain packet footer bytes!";
                base.Error(errMsg);
                return new PacketSendResult
                {
                    OriginalPacketId = packetId,
                    Ok = false,
                    Message = errMsg,
                };
            }

            // send packet byte array message

            //serialPort.DiscardOutBuffer();



            serialPort.Write(_comPortCommandConfig.PacketHeaderAsBytes, 0, _comPortCommandConfig.PacketHeaderAsBytes.Length);
            serialPort.Write(packetAsBytes, 0, packetAsBytes.Length);
            serialPort.Write(_comPortCommandConfig.PacketFooterAsBytes, 0, _comPortCommandConfig.PacketFooterAsBytes.Length);

            var timeOutDateTime = DateTime.UtcNow + base._timeout;

            while (DateTime.UtcNow < timeOutDateTime)
            {
                var packetReceived = await _packetReceiver.TryReceivePacket();

                if (packetReceived?.PacketType == PacketEnvelope<PayloadTypes>.PacketTypes.ResponsePacket)
                {
                    if (packetReceived.Id == packetId)
                    {
                        if (string.IsNullOrEmpty(packetReceived.Payload))
                        {
                            return new PacketSendResult
                            {
                                Ok = true,
                                OriginalPacketId = packetReceived.Id,
                                Message = null,
                            };
                        }
                        else
                        {
                            return new PacketSendResult
                            {
                                Ok = false,
                                OriginalPacketId = packetReceived.Id,
                                Message = $"Problem sending packet {packetReceived.Id}: {payload}",
                            };

                        }
                    }
                }
            }

            base.Error($"Timeout sending packet {packetId} to {_serialPortName}");

            return new PacketSendResult
            {
                Ok = false,
                OriginalPacketId = packetId,
                Message = "Timeout",
            };
        }


    }
}

// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using PacketLogistics.ComPorts;
using static Awb.Core.Clients.IAwbClient;

namespace Awb.Core.Clients
{
    public class Esp32ComPortClient : IAwbClient, IDisposable
    {
        private PacketSenderReceiverComPort _comPortReceiver;

        private bool _connected;

        public uint ClientId { get; }

        public string FriendlyName { get; }

        public EventHandler<IAwbClient.ReceivedEventArgs> Received { get; set; }

        public Esp32ComPortClient(string comPortName, uint clientId)
        {
            if (string.IsNullOrWhiteSpace(comPortName)) throw new ArgumentNullException(nameof(comPortName));

            this.ClientId = clientId;
            this.FriendlyName = $"com:{comPortName}:{clientId}";

            _comPortReceiver = new PacketSenderReceiverComPort(
            serialPortName: comPortName,
            clientID: clientId,
            comPortCommandConfig: new AwbEsp32ComportClientConfig());
        }

        public async Task<bool> Init()
        {
            _comPortReceiver.PacketReceived += _comPortReceiver_PacketReceived;
            _connected = await _comPortReceiver.Connect();
            return _connected;
        }

        private void _comPortReceiver_PacketReceived(object? sender, PacketLogistics.PacketSenderReceiver.PacketReceivedEventArgs e)
        {
            if (Received != null)
                Received(this, new IAwbClient.ReceivedEventArgs(payload: e.Payload));
        }

        public void Dispose()
        {
            _connected = false;
            _comPortReceiver.PacketReceived -= _comPortReceiver_PacketReceived;
            _comPortReceiver?.Dispose();
        }

        public async Task<SendResult> Send(byte[] payload)
        {
            if (!_connected) return new SendResult(false, "not connected, please run Init() before using Esp32ComPortClient!", null);
            var result = await _comPortReceiver.SendPacket(payload);
            return new SendResult(result.Ok, result.Message, $"PacketID:{result.OriginalPacketId};Time:{result.OriginalPacketTimestampUtc}");
        }


    }
}

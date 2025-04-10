// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

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

        public EventHandler<IAwbClient.ReceivedEventArgs>? Received { get; set; }
        public EventHandler<string>? OnError { get; set; }

        public Esp32ComPortClient(string comPortName, uint clientId)
        {
            if (string.IsNullOrWhiteSpace(comPortName)) throw new ArgumentNullException(nameof(comPortName));

            this.ClientId = clientId;
            this.FriendlyName = $"com:{comPortName}:{clientId}";

            _comPortReceiver = new PacketSenderReceiverComPort(
            serialPortName: comPortName,
            clientID: clientId,
            comPortCommandConfig: new AwbEsp32ComportClientConfig());
            _comPortReceiver.ErrorOccured += (sender, args) => OnError?.Invoke(this, args.Message);
        }

        public async Task<bool> InitAsync()
        {
            _comPortReceiver.PacketReceived += _comPortReceiver_PacketReceived;
            _connected = await _comPortReceiver.Connect();
            return _connected;
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

        private void _comPortReceiver_PacketReceived(object? sender, PacketLogistics.PacketSenderReceiver.PacketReceivedEventArgs e)
        {
            Received?.Invoke(this, new IAwbClient.ReceivedEventArgs(payload: e.Payload));
        }

    }
}

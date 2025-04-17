// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Clients.Models;
using PacketLogistics.ComPorts;

namespace Awb.Core.Clients
{
    public class Esp32ComPortClient : IAwbClient, IDisposable
    {
        private PacketSenderReceiverComPort _comPortReceiver;

        public bool Connected { get; private set; } = false;

        public uint ClientId { get; }

        public string FriendlyName { get; }

        public DateTime? LastErrorUtc { get; private set; }

        public EventHandler<ReceivedEventArgs>? Received { get; set; }
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
            _comPortReceiver.ErrorOccured += (sender, args) =>
            {
                LastErrorUtc = DateTime.UtcNow;
                OnError?.Invoke(this, args.Message);
            };
        }

        public async Task<bool> InitAsync()
        {
            _comPortReceiver.PacketReceived += _comPortReceiver_PacketReceived;
            Connected = await _comPortReceiver.Connect();
            return Connected;
        }

        public void Dispose()
        {
            Connected = false;
            _comPortReceiver.PacketReceived -= _comPortReceiver_PacketReceived;
            _comPortReceiver?.Dispose();
        }

        public async Task<SendResult> Send(byte[] payload)
        {
            if (!Connected) return new SendResult(false, errorMessage: "not connected, please run Init() before using Esp32ComPortClient!", resultPlayload: null, debugInfos: null);
            var result = await _comPortReceiver.SendPacket(payload);

            if (result.Ok == false)
                return new SendResult(ok: false, errorMessage: result.Message, resultPlayload: null, $"PacketID:{result.OriginalPacketId};Time:{result.OriginalPacketTimestampUtc}");

            return new SendResult(ok: true, errorMessage: null, resultPlayload: result.Message, debugInfos: $"PacketID:{result.OriginalPacketId};Time:{result.OriginalPacketTimestampUtc}");
        }

        private void _comPortReceiver_PacketReceived(object? sender, PacketLogistics.PacketSenderReceiver.PacketReceivedEventArgs e)
        {
            Received?.Invoke(this, new ReceivedEventArgs(payload: e.Payload));
        }

    }
}

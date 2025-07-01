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
        public enum PayloadTypes
        {
            JsonData
        }

        private PacketSenderReceiverComPort<PayloadTypes> _comPortReceiver;

        public bool Connected { get; private set; } = false;

        public uint ClientId { get; }

        public string FriendlyName { get; }

        public DateTime? LastErrorUtc { get; private set; }

        public EventHandler<ReceivedEventArgs>? Received { get; set; }
        public EventHandler<string>? OnError { get; set; }
        public EventHandler<string>? PacketSending { get; set; }

        public Esp32ComPortClient(string comPortName, uint clientId)
        {
            if (string.IsNullOrWhiteSpace(comPortName)) throw new ArgumentNullException(nameof(comPortName));

            this.ClientId = clientId;
            this.FriendlyName = $"com:{comPortName}:{clientId}";

            _comPortReceiver = new PacketSenderReceiverComPort<PayloadTypes>(
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
            Connected = await _comPortReceiver.Connect();
            return Connected;
        }
        public void Dispose()
        {
            Connected = false;
            _comPortReceiver?.Dispose();
        }

        public async Task<SendResult> Send(string payload, string? debugInfo)
        {
            if (!Connected) return new SendResult(false, errorMessage: "not connected, please run Init() before using Esp32ComPortClient!", resultPlayload: null, debugInfos: null);

            if (!string.IsNullOrWhiteSpace(debugInfo))
                this.PacketSending?.Invoke(this, $"Send packet info\r\n{debugInfo}");

            var result = await _comPortReceiver.SendPacket(payloadType: PayloadTypes.JsonData, payload: payload);

            if (result.Ok == false)
            {
                var details = $"PacketID:{result.OriginalPacketId}";
                OnError?.Invoke(this, $"Error sending packet: {result.ErrorMessage} / {details}");
                return new SendResult(ok: false, errorMessage: result.ErrorMessage, resultPlayload: null, details);
            }

            return new SendResult(ok: true, errorMessage: null, resultPlayload: result.ReturnPayload, debugInfos: $"PacketID:{result.OriginalPacketId}");
        }
        private void _comPortReceiver_PacketReceived(object? sender, PacketLogistics.PacketSenderReceiver<PayloadTypes>.PacketReceivedEventArgs e)
        {
            Received?.Invoke(this, new ReceivedEventArgs(payload: e.Payload));
        }

    }
}

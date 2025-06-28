// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Clients.Models;

namespace Awb.Core.Clients
{
    internal class Esp32WifiClient : IAwbClient
    {
        public uint ClientId => throw new NotImplementedException();

        public string FriendlyName => throw new NotImplementedException();


        public EventHandler<string>? OnError { get; set; }

        public DateTime? LastErrorUtc => throw new NotImplementedException();

        public EventHandler<ReceivedEventArgs>? Received { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public EventHandler<string>? PacketSending { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public async Task<bool> InitAsync()
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        public Task<SendResult> Send(String payload)
        {
            throw new NotImplementedException();
        }

        public Task<SendResult> Send(String payload, string? debugInfos)
        {
            throw new NotImplementedException();
        }
    }
}

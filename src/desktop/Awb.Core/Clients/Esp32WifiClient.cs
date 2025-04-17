// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

namespace Awb.Core.Clients
{
    internal class Esp32WifiClient : IAwbClient
    {
        public uint ClientId => throw new NotImplementedException();

        public string FriendlyName => throw new NotImplementedException();

        public EventHandler<IAwbClient.ReceivedEventArgs>? Received { get; set; }

        public EventHandler<string>? OnError { get; set; }

        public async Task<bool> InitAsync()
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        public async Task<IAwbClient.SendResult> Send(byte[] payload)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }
    }
}

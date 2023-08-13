// AnimatronicWorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

namespace Awb.Core.Clients
{
    internal class Esp32BluetoothClient : IAwbClient
    {
        public uint ClientId => throw new NotImplementedException();

        public string FriendlyName => throw new NotImplementedException();

        public EventHandler<IAwbClient.ReceivedEventArgs> Received => throw new NotImplementedException();

        public async Task<bool> Init()
        {
            throw new NotImplementedException();
        }

        public async Task<IAwbClient.SendResult> Send(byte[] payload)
        {
            throw new NotImplementedException();
        }
    }
}

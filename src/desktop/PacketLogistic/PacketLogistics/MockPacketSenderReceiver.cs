// Communicate between different devices on dotnet or arduino via COM port or Wifi
// https://github.com/Springwald/PacketLogistics
//
// (C) 2023 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using PacketLogistics.ComPorts.ComportPackets;

namespace PacketLogistics
{
    public class MockPacketSenderReceiver<PayloadTypes> : PacketSenderReceiver<PayloadTypes> where PayloadTypes : Enum
    {
        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        protected override async Task<bool> ConnectInternal()
        {
            await Task.Delay(100); 
            return true;
        }

        protected override async Task<PacketSendResult> SendPacketInternal(PayloadTypes payloadType, string payload)
        {
            await Task.Delay(100 + new Random().Next(200));
            return new PacketSendResult
            {
                OriginalPacketId = null,
                Ok = true,
                Message = null,
            };
        }
    }
}

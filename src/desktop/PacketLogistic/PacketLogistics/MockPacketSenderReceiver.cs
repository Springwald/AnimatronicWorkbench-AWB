// Send and receivce data to/from ESP-32 microcontroller
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

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
            };
        }
    }
}

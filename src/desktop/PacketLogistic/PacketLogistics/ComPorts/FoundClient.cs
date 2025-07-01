// Send and receivce data to/from ESP-32 microcontroller
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

namespace PacketLogistics.ComPorts
{
    public class FoundClient
    {
        public uint ClientId { get; }
        public string ComPortName { get; }
        public string Caption { get; }
        public string DeviceId { get; }

        public FoundClient(uint clientId, string comPortName, string caption, string deviceId)
        {
            this.ClientId = clientId;
            this.ComPortName = comPortName;
            this.Caption = caption;
            this.DeviceId = deviceId;
        }
    }
}

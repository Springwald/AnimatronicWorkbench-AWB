// Send and receivce data to/from ESP-32 microcontroller
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

namespace PacketLogistics.Tools
{
    internal class ComPortInfo
    {
        public string DeviceId { get; }
        public string Caption { get; }
        public string ComPort { get; }
        public string? CaptionCleaned => Caption?.Split(new char[] { '(' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim();


        // the constructor setting all properties
        public ComPortInfo(string deviceId, string caption, string comPort)
        {
            this.DeviceId = deviceId;
            this.Caption = caption;
            this.ComPort = comPort;
        }

    }
}

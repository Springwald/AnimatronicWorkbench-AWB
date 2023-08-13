// Communicate between different devices on dotnet or arduino via COM port or Wifi
// https://github.com/Springwald/PacketLogistics
//
// (C) 2023 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

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

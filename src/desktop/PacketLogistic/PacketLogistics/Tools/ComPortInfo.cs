// Communicate between different devices on dotnet or arduino via COM port or Wifi
// https://github.com/Springwald/PacketLogistics
//
// (C) 2023 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

namespace PacketLogistics.Tools
{
    internal class ComPortInfo
    {
        public string DeviceId { get;  }
        public string Caption { get;  }
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

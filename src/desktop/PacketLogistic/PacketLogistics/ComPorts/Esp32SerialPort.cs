// Communicate between different devices on dotnet or arduino via COM port or Wifi
// https://github.com/Springwald/PacketLogistics
//
// (C) 2023 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using System.IO.Ports;

namespace PacketLogistics.ComPorts
{
    internal class Esp32SerialPort : SerialPort
    {
        public Esp32SerialPort(string portName)
        {
            this.PortName = portName;

            // settings for ESP32 UART
            this.BaudRate = 115200;
            //this.Parity = Parity.None;
            //this.DataBits = 8;
            // this.StopBits = StopBits.One;
            // this.Handshake = Handshake.XOnXOff;

            // Set the read/write timeouts
            // this.ReadTimeout = 500;
            // this.WriteTimeout = 500;
        }
    }
}

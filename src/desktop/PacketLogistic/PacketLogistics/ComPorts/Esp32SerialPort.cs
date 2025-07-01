// Send and receivce data to/from ESP-32 microcontroller
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using System.IO.Ports;

namespace PacketLogistics.ComPorts
{
    internal class Esp32SerialPort : SerialPort
    {
        public Esp32SerialPort(string portName)
        {
            this.PortName = portName;

            // settings for ESP32 UART
            // this.BaudRate =  115200;
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

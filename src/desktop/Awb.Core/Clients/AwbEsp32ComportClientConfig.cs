// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using PacketLogistics.ComPorts;

namespace Awb.Core.Clients
{
    internal class AwbEsp32ComportClientConfig : ComPortCommandConfig
    {
        public AwbEsp32ComportClientConfig() : base(packetIdentifier: "awb")
        {
        }
    }
}

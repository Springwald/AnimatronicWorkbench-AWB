// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using PacketLogistics.ComPorts;

namespace Awb.Core.Clients
{
    internal class AwbEsp32ComportClientConfig : ComPortCommandConfig
    {
        public AwbEsp32ComportClientConfig() :
            base(packetHeader: "AWB", headerStartByte: 255, headerEndByte: 254)
        {
        }
    }
}

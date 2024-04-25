﻿// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

namespace Awb.Core.Export.ExporterParts
{
    public class WifiConfigData
    {
        public string WlanSSID { get; }
        public string WlanPassword { get; }
        public WifiConfigData(string wifiSSID, string wifiPassword)
        {
            WlanSSID = wifiSSID;
            WlanPassword = wifiPassword;
        }
    }
}

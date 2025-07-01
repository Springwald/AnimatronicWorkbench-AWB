// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

namespace Awb.Core.Export.ExporterParts.ExportData
{
    public class WifiConfigExportData
    {
        public required string WlanSSID { get; init; }
        public required string WlanPassword { get; init; }
    }
}

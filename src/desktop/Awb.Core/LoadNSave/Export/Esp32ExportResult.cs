// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

namespace Awb.Core.LoadNSave.Export
{
    public class Esp32ExportResult
    {
        public bool Ok { get; set; }
        public string? Message { get; set; }
        public string? Code { get; set; }
    }
}

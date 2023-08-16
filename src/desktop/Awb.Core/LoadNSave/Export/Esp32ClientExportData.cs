// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Configs;
using Awb.Core.Timelines;

namespace Awb.Core.LoadNSave.Export
{
    public class Esp32ClientExportData
    {
        public TimelineData[]? TimelineData { get; set; }
        public TimelineState[]? TimelineStates { get; set; }
        public StsServoConfig[]? StsServoConfigs { get; set; }
        public string? ProjectName { get; set; }
    }
}

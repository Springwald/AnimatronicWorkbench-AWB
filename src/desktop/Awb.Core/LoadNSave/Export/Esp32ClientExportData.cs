// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Project;
using Awb.Core.Timelines;

namespace Awb.Core.LoadNSave.Export
{
    public class Esp32ClientExportData
    {
        public TimelineData[]? TimelineData { get; set; }
        public TimelineState[]? TimelineStates { get; set; }
        public StsServoConfig[]? StsServoConfigs { get; set; }
        public StsServoConfig[]? ScsServoConfigs { get; set; }
        public Pca9685PwmServoConfig[]? Pca9685PwmServoConfigs { get; set; }
        public Mp3PlayerYX5300Config[]? Mp3PlayerYX5300Configs { get; set; }
        public InputConfig[]? InputConfigs { get; set; }

        public string? ProjectName { get; set; }
    }
}

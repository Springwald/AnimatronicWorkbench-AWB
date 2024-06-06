﻿// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Project;

namespace Awb.Core.Export.ExporterParts
{
    public class ProjectExportData
    {
        public required string ProjectName { get; init; }

        public required IEnumerable<TimelineExportData> TimelineData { get; init; }
        public required IEnumerable<TimelineState> TimelineStates { get; init; }
        public required IEnumerable<StsServoConfig> StsServoConfigs { get; init; }
        public required IEnumerable<StsServoConfig> ScsServoConfigs { get; init; }
        public required IEnumerable<Pca9685PwmServoConfig> Pca9685PwmServoConfigs { get; init; }
        public required IEnumerable<Mp3PlayerYX5300Config> Mp3PlayerYX5300Configs { get; init; }
        public required IEnumerable<InputConfig> InputConfigs { get; init; }
    }
}

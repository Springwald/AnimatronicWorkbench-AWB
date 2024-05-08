// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.LoadNSave.Export;
using Awb.Core.Project;

namespace Awb.Core.Export.ExporterParts
{
    public class ProjectExportData
    {
        public required string ProjectName { get; init; }

        public required TimelineExportData[] TimelineData { get; init; }
        public required TimelineState[] TimelineStates { get; init; }
        public required StsServoConfig[] StsServoConfigs { get; init; }
        public required StsServoConfig[] ScsServoConfigs { get; init; }
        public required Pca9685PwmServoConfig[] Pca9685PwmServoConfigs { get; init; }
        public required Mp3PlayerYX5300Config[] Mp3PlayerYX5300Configs { get; init; }
        public required InputConfig[] InputConfigs { get; init; }
    }
}

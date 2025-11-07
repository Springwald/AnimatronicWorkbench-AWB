// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Project.Clients;
using Awb.Core.Project.Servos;
using Awb.Core.Project.Various;

namespace Awb.Core.Export.ExporterParts.ExportData
{
    public class ProjectExportData
    {
        public required string ProjectName { get; init; }

        public required IEnumerable<TimelineExportData> TimelineData { get; init; }
        public required IEnumerable<TimelineState> TimelineStates { get; init; }
        public required IEnumerable<StsFeetechServoConfig> StsServoConfigs { get; init; }
        public required IEnumerable<ScsFeetechServoConfig> ScsServoConfigs { get; init; }
        public required IEnumerable<Pca9685PwmServoConfig> Pca9685PwmServoConfigs { get; init; }
        public required IEnumerable<Mp3PlayerYX5300Config> Mp3PlayerYX5300Configs { get; init; }
        public required IEnumerable<Mp3PlayerDfPlayerMiniConfig> Mp3PlayerDfPlayerMiniConfigs { get; init; }
        public required IEnumerable<InputConfig> InputConfigs { get; init; }
        public required Esp32ClientHardwareConfig Esp32ClientHardwareConfig { get; init; }


    }
}

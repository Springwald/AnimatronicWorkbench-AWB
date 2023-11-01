// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

namespace Awb.Core.Configs
{
    public class AwbProject
    {
        public string Info { get; set; } = "Animatronic Workbench Project | https://daniel.springwald.de/post/AnimatronicWorkbench";

        public string Title { get; set; }

        public string ProjectFolder { get; set; }

        public Pca9685PwmServoConfig[]? Pca9685PwmServos { get; set; }

        public StsServoConfig[]? StsServos { get; set; }

        public TimelineState[]? TimelinesStates { get; set; }

        public string? AutoPlayEsp32ExportFolder { get; set; }

        public AwbProject(string title, string projectFolder)
        {
            Title = title;
            ProjectFolder = projectFolder;
        }

        public static implicit operator string(AwbProject v)
        {
            throw new NotImplementedException();
        }
    }
}

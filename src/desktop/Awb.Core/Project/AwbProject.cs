// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Project;
using Awb.Core.Sounds;

namespace Awb.Core.Configs
{
    public class AwbProject
    {
        public string Info { get; set; } = "Animatronic Workbench Project | https://daniel.springwald.de/post/AWB/AnimatronicWorkbench";

        public string Title { get; set; }

        public string ProjectFolder { get; set; }

        public Pca9685PwmServoConfig[]? Pca9685PwmServos { get; set; }

        public StsServoConfig[]? StsServos { get; set; }
        public StsServoConfig[]? ScsServos { get; set; }

        public Mp3PlayerYX5300Config? Mp3PlayerYX5300 { get; set; } 

        public TimelineState[]? TimelinesStates { get; set; }

        public string? AutoPlayEsp32ExportFolder { get; set; }

        public Sound[] Sounds { get; }

        public AwbProject(string title, string projectFolder)
        {
            Title = title;
            ProjectFolder = projectFolder;
            Sounds = new SoundManager(Path.Combine(projectFolder, "audio")).Sounds;
        }

        
    }
}

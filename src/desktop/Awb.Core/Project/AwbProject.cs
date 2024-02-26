// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Project;
using Awb.Core.Sounds;

namespace Awb.Core.Configs
{
    public class AwbProject
    {
        private Sound[]? _sounds;
        public string? _projectFolder;

        public string Info { get; set; } = "Animatronic Workbench Project | https://daniel.springwald.de/post/AWB/AnimatronicWorkbench";

        public string Title { get; set; }


        public Pca9685PwmServoConfig[]? Pca9685PwmServos { get; set; }

        public StsServoConfig[]? StsServos { get; set; }
        public StsServoConfig[]? ScsServos { get; set; }

        public Mp3PlayerYX5300Config? Mp3PlayerYX5300 { get; set; }

        public TimelineState[]? TimelinesStates { get; set; }

        public string? AutoPlayEsp32ExportFolder { get; set; }

        public Sound[] Sounds => _sounds ?? throw new Exception("Sounds not set! Have you set the project folder?");
        public string ProjectFolder => _projectFolder ?? throw new Exception("Project folder not set!");

        public AwbProject(string title)
        {
            Title = title;
        }

        public void SetProjectFolder(string folder)
        {
            if (!Path.Exists(folder)) throw new DirectoryNotFoundException(folder);
            _projectFolder = folder;
            _sounds = new SoundManager(Path.Combine(_projectFolder, "audio")).Sounds;
        }


    }
}

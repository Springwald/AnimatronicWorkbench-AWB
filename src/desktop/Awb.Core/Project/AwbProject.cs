﻿// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Services;
using Awb.Core.Sounds;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Awb.Core.Project
{
    public class AwbProject
    {
        private string _wifiSsid;

        private ITimelineDataService? _timelineDataService;

        private Sound[]? _sounds;

        public string? _projectFolder;

        public string? Info { get; set; }

        public string Title { get; set; }

        public string WifiSsid
        {
            get => _wifiSsid ?? $"AWB-{Title}"; // todo: remove invalid chars
            set => _wifiSsid = value;
        }
        public string WifiPassword { get; set; } = "awb12345";

        public required ObservableCollection<Pca9685PwmServoConfig> Pca9685PwmServos { get; init; }
        public required ObservableCollection<StsServoConfig> StsServos { get; init; }
        public required ObservableCollection<StsServoConfig> ScsServos { get; init; }
        public required ObservableCollection<Mp3PlayerYX5300Config> Mp3PlayersYX5300 { get; init; }
        public required ObservableCollection<TimelineState> TimelinesStates { get; init; }
        public required  ObservableCollection<InputConfig> Inputs { get; init; }

        public int ItemsPerBank { get; set; } = 8;

        [JsonIgnore]
        public ITimelineDataService TimelineDataService => _timelineDataService ?? throw new Exception("TimelineDataService not set! Have you set the project folder?");

        [JsonIgnore]
        public Sound[] Sounds => _sounds ?? throw new Exception("Sounds not set! Have you set the project folder?");

        [JsonIgnore]
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
            _timelineDataService = new TimelineDataServiceByJsonFiles(_projectFolder);
        }


    }
}

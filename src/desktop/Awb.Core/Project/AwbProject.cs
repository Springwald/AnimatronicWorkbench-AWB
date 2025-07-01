// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Project.Clients;
using Awb.Core.Project.Servos;
using Awb.Core.Project.Various;
using Awb.Core.Services;
using Awb.Core.Sounds;
using System.Text.Json.Serialization;

namespace Awb.Core.Project
{
    public class AwbProject : AwbProjectFileVersion
    {
        private ITimelineDataService? _timelineDataService;
        private Sound[]? _sounds;
        public string? _projectFolder;

        public ProjectMetaData ProjectMetaData { get; set; } = new ProjectMetaData();

        public Esp32ClientHardwareConfig Esp32ClientHardware { get; set; } = new Esp32ClientHardwareConfig { ClientId = 1 };

        public IProjectObjectListable[] AdditionalClients { get; set; } = [];

        public Pca9685PwmServoConfig[] Pca9685PwmServos { get; set; } = [];
        public StsFeetechServoConfig[] StsServos { get; set; } = [];
        public ScsFeetechServoConfig[] ScsServos { get; set; } = [];
        public Mp3PlayerYX5300Config[] Mp3PlayersYX5300 { get; set; } = [];
        public Mp3PlayerDfPlayerMiniConfig[] Mp3PlayersDFPlayerMini { get; set; } = [];
        public TimelineState[] TimelinesStates { get; set; } = [];
        public InputConfig[] Inputs { get; set; } = [];

        public int ItemsPerBank { get; set; } = 8;

        [JsonIgnore]
        public ITimelineDataService TimelineDataService => _timelineDataService ?? throw new Exception("TimelineDataService not set! Have you set the project folder?");

        [JsonIgnore]
        public Sound[] Sounds => _sounds ?? throw new Exception("Sounds not set! Have you set the project folder?");

        [JsonIgnore]
        public string ProjectFolder => _projectFolder ?? throw new Exception("Project folder not set!");

        public void SetProjectFolder(string folder)
        {
            if (!Path.Exists(folder)) throw new DirectoryNotFoundException(folder);
            _projectFolder = folder;
            _sounds = new SoundManager(Path.Combine(_projectFolder, "audio")).Sounds;
            _timelineDataService = new TimelineDataServiceByJsonFiles(_projectFolder, _sounds);
        }
        public IEnumerable<IProjectObjectListable> GetAllListableObjects()
        {
            yield return ProjectMetaData;
            yield return Esp32ClientHardware;
            foreach (var item in Pca9685PwmServos) yield return item;
            foreach (var item in StsServos) yield return item;
            foreach (var item in ScsServos) yield return item;
            foreach (var item in Mp3PlayersYX5300) yield return item;
            foreach (var item in Mp3PlayersDFPlayerMini) yield return item;
            foreach (var item in TimelinesStates) yield return item;
            foreach (var item in Inputs) yield return item;
        }
    }
}

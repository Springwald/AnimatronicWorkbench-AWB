// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Project;
using AwbStudio.StudioSettings;
using System;
using System.IO;
using System.Text.Json;

namespace AwbStudio.Projects
{
    public interface IProjectManagerService
    {
        AwbProject ActualProject { get; }
        bool ExistProject(string projectPath);
        bool OpenProject(string projectFolder, out string[] errorMessages);
        bool SaveProject(AwbProject project, string projectFolder);
    }

    public class ProjectManagerService : IProjectManagerService
    {
        private readonly IAwbStudioSettingsService _awbStudioSettingsService;

        public AwbProject? ActualProject { get; private set; }

        public ProjectManagerService(IAwbStudioSettingsService awbStudioSettingsService)
        {
            _awbStudioSettingsService = awbStudioSettingsService;
        }

        public bool ExistProject(string projectFolder)
        {
            if (!Directory.Exists(projectFolder)) return false;
            return File.Exists(ProjectConfigFilename(projectFolder));
        }

        public bool SaveProject(AwbProject project, string projectFolder)
        {
            if (!Directory.Exists(projectFolder)) return false;

            // load project config from folder
            var options = new JsonSerializerOptions()
            {
                WriteIndented = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never,
                PropertyNameCaseInsensitive = true,
            };
            project.SetProjectFolder(projectFolder);
            var jsonStr = JsonSerializer.Serialize<AwbProject>(project, options);
            File.WriteAllText(ProjectConfigFilename(projectFolder), jsonStr);
            return true;
        }

        public bool OpenProject(string projectFolder, out string[] errorMessages)
        {
            if (!Directory.Exists(projectFolder))
            {
                errorMessages = new string[] { $"Project folder '{projectFolder}' does not exist." };
                return false;
            }

            // load project config from folder
            var options = new JsonSerializerOptions()
            {
                WriteIndented = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never,
                PropertyNameCaseInsensitive = true,
            };

            AwbProject? projectConfig = null;
            try
            {
                var jsonStr = File.ReadAllText(ProjectConfigFilename(projectFolder));
                projectConfig = JsonSerializer.Deserialize<AwbProject>(jsonStr, options);
            }
            catch (Exception ex)
            {
                errorMessages = new string[] { $"Project config file '{ProjectConfigFilename(projectFolder)}' could not be loaded: " + ex.Message };
                return false;
            }

            if (projectConfig == null)
            {
                errorMessages = new string[] { $"Project config file '{ProjectConfigFilename(projectFolder)}' could not be loaded: Deserialized == null" };
                return false;
            }
            projectConfig.SetProjectFolder(projectFolder);
            this.ActualProject = projectConfig;

            _awbStudioSettingsService.StudioSettings.AddLastProjectFolder(projectFolder);
            _awbStudioSettingsService.SaveSettings();

            errorMessages = new string[] { };
            return true;
        }

        private string ProjectConfigFilename(string projectFolder) => Path.Combine(projectFolder, "AwbProject.json");

    }
}

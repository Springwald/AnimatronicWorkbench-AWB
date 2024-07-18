// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Project;
using AwbStudio.StudioSettings;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Media;
using static AwbStudio.Projects.IProjectManagerService;

namespace AwbStudio.Projects
{
    public interface IProjectManagerService
    {
        public record OpenProjectResult
        {
            public required bool Success { get; init; }
            public required string[] ErrorMessages { get; init; }

            public static OpenProjectResult SuccessResult => new() { Success = true, ErrorMessages = [] };
            public static OpenProjectResult ErrorResult(string[] errorMessages) => new() { Success = false, ErrorMessages = errorMessages };
        }

        AwbProject? ActualProject { get; }
        bool ExistProject(string projectPath);
        Task<OpenProjectResult> OpenProjectAsync(string projectFolder);
        Task<bool> SaveProjectAsync(AwbProject project, string projectFolder);
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

        public async Task<bool> SaveProjectAsync(AwbProject project, string projectFolder)
        {
            if (!Directory.Exists(projectFolder)) return false;

            if (await CreateProjectSubDirectories(projectFolder: projectFolder) == false) return false;

            // save project to folder
            var options = new JsonSerializerOptions()
            {
                WriteIndented = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never,
                PropertyNameCaseInsensitive = true,
            };
            project.SetProjectFolder(projectFolder);
            var jsonStr = JsonSerializer.Serialize<AwbProject>(project, options);
            await File.WriteAllTextAsync(ProjectConfigFilename(projectFolder), jsonStr);
            return true;
        }

        public async Task<bool> CreateProjectSubDirectories(string projectFolder)
        {
            var folders = new[] { @"audio\SDCard\01", @"CustomCode\EspClient1" };

            foreach (var folder in folders)
            {
                var path = Path.Combine(projectFolder, folder);
                if (!Directory.Exists(path))
                {
                    try
                    {
                        Directory.CreateDirectory(path);
                    }
                    catch (Exception ex)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public async Task<OpenProjectResult> OpenProjectAsync(string projectFolder)
        {
            if (!Directory.Exists(projectFolder))
                return  OpenProjectResult.ErrorResult([$"Project folder '{projectFolder}' does not exist."]);

            var projectFilename = ProjectConfigFilename(projectFolder);

            if (!File.Exists(projectFilename))
                return OpenProjectResult.ErrorResult([$"Project config file '{projectFilename}' does not exist."]);

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
                return OpenProjectResult.ErrorResult([$"Project config file '{ProjectConfigFilename(projectFolder)}' could not be loaded: " + ex.Message]);
            }

            if (projectConfig == null)
                return OpenProjectResult.ErrorResult([$"Project config file '{ProjectConfigFilename(projectFolder)}' could not be loaded: Deserialized == null"]);

            projectConfig.SetProjectFolder(projectFolder);
            this.ActualProject = projectConfig;

            _awbStudioSettingsService.StudioSettings.AddLastProjectFolder(projectFolder);
            await _awbStudioSettingsService.SaveSettingsAsync();

            return OpenProjectResult.SuccessResult;
        }

        private string ProjectConfigFilename(string projectFolder) => Path.Combine(projectFolder, "AwbProject.awbprj");
    }
}

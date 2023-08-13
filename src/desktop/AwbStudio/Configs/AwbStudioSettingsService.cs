// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using System;
using System.IO;
using System.Text.Json;
using System.Windows.Shapes;

namespace AwbStudio.Configs
{
    public interface IAwbStudioSettingsService
    {
        AwbStudioSettings StudioSettings { get; }

        bool SaveSettings();
    }

    public class AwbStudioSettingsService : IAwbStudioSettingsService
    {
        private AwbStudioSettings _studioSettings;

        // set SettingsFilename to the path inside the user document folder
        private string SettingsFilename => System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "AwbStudio", "AwbStudioSettings.json");

        public AwbStudioSettingsService()
        {
            if (File.Exists(SettingsFilename))
            {
                // read settings from file
                var options = new JsonSerializerOptions()
                {
                    WriteIndented = true,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                    PropertyNameCaseInsensitive = true,
                };
                var jsonStr = File.ReadAllText(SettingsFilename);
                var settings = JsonSerializer.Deserialize<AwbStudioSettings>(jsonStr, options);
                if (settings == null) settings = new AwbStudioSettings();
                this._studioSettings = settings;
            }
            else
            {
                this._studioSettings = new AwbStudioSettings();
            }
        }

        public bool SaveSettings()
        {
            var options = new JsonSerializerOptions()
            {
                WriteIndented = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                PropertyNameCaseInsensitive = true,
            };
            var jsonStr = JsonSerializer.Serialize<AwbStudioSettings>(this._studioSettings, options);

            var path = System.IO.Path.GetDirectoryName(SettingsFilename);
            if (path == null) { return false; }
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            File.WriteAllText(SettingsFilename, jsonStr);
            return true;
        }

        public AwbStudioSettings StudioSettings => _studioSettings;
    }
}

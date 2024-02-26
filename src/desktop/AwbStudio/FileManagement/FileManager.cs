// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Configs;
using Awb.Core.LoadNSave.TimelineLoadNSave;
using Awb.Core.Sounds;
using Awb.Core.Timelines;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AwbStudio.FileManagement
{

    public class FileManager
    {
        private readonly AwbProject _project;
        internal readonly string ProjectTitle;

        public FileManager(AwbProject project)
        {
            _project = project;
            ProjectTitle = project.Title;
        }

        public IEnumerable<string> TimelineFilenames => Directory.GetFiles(_project.ProjectFolder, "*.awbtl");

        public string GetTimelineFilename(string timelineName) => Path.Combine(_project.ProjectFolder, $"{timelineName}.awbtl");

        public TimelineData? LoadTimelineData(string filename)
        {
            if (!File.Exists(filename)) return null;
            var jsonString = System.IO.File.ReadAllText(filename);
            var saveFormat = JsonSerializer.Deserialize<TimelineSaveFormat>(jsonString, _jsonOptions);
            if (saveFormat == null) return null;
            var timelineData = TimelineSaveFormat.ToTimelineData(saveFormat);
            var timelineTitle = Path.GetFileNameWithoutExtension(filename);
            timelineData.Title = timelineTitle;
            return timelineData;
        }

        private JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            IgnoreReadOnlyFields = true,
            IgnoreReadOnlyProperties = true,
            PropertyNameCaseInsensitive = false,
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
            WriteIndented = true,
            Converters = {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            },
        };
 

        public bool SaveTimelineData(string filename, TimelineData data)
        {
            var saveFormat = TimelineSaveFormat.FromTimelineData(data);
            var jsonString = JsonSerializer.Serialize(saveFormat, _jsonOptions);
            System.IO.File.WriteAllText(filename, jsonString);
            return true;
        }

        public TimelineMetaData? GetTimelineMetaData(string filename)
        {
            var data = LoadTimelineData(filename);
            if (data == null) return null;

            var state = _project?.TimelinesStates?.SingleOrDefault(s => s.Id == data.TimelineStateId);
            var stateName = state?.Name;
            return new TimelineMetaData(title: data.Title, stateId: data.TimelineStateId, stateName: stateName ?? $"unknown StateId {data.TimelineStateId}");
        }
    }
}

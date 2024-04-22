// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.LoadNSave.TimelineLoadNSave;
using Awb.Core.Project;
using Awb.Core.Timelines;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AwbStudio.FileManagement
{
    public class TimelineFileManager
    {
        private ITimelineMetaDataService? _timelineMetaDataService;
        private readonly AwbProject _project;
        internal readonly string ProjectTitle;

        public ITimelineMetaDataService TimelineMetaDataService
        {
            get
            {
                _timelineMetaDataService ??= new TimelineMetaDataService(this);
                return _timelineMetaDataService;
            }
        }

        public TimelineFileManager(AwbProject project)
        {
            _project = project;
            ProjectTitle = project.Title;
            ConvertOldFilenamesIfNeeded(deleteOldFiles: true);
        }

        public IEnumerable<string> TimelineFilenamesOld => Directory.GetFiles(_project.ProjectFolder, "*.awbtl");
        public IEnumerable<string> TimelineRawFilenames => Directory.GetFiles(_project.ProjectFolder, "*.awbt");

        public IEnumerable<string> TimelineIds => Directory.GetFiles(_project.ProjectFolder, "*.awbt").Select(f => Path.GetFileNameWithoutExtension(f));

        public string GetTimelineFilenameById(string timelineId) => Path.Combine(_project.ProjectFolder, $"{timelineId}.awbt");

        public TimelineData? LoadTimelineDataById(string timelineId)
        {
            var filename = GetTimelineFilenameById(timelineId);
            return LoadTimelineDataByFilename(filename);
        }

        private TimelineData? LoadTimelineDataByFilename(string filename)
        {
            if (!File.Exists(filename)) return null;
            var jsonString = System.IO.File.ReadAllText(filename);
            var saveFormat = JsonSerializer.Deserialize<TimelineSaveFormat>(jsonString, _jsonOptions);
            if (saveFormat == null) return null;
            var timelineData = TimelineSaveFormat.ToTimelineData(saveFormat);
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


        public bool SaveTimelineData(TimelineData data)
        {
            if (string.IsNullOrWhiteSpace(data.Id)) data.Id = System.Guid.NewGuid().ToString();
            var filename = GetTimelineFilenameById(data.Id);
            var saveFormat = TimelineSaveFormat.FromTimelineData(data);
            var jsonString = JsonSerializer.Serialize(saveFormat, _jsonOptions);
            System.IO.File.WriteAllText(filename, jsonString);
            _timelineMetaDataService?.ClearCache(data.Id);
            return true;
        }

        public TimelineMetaData? GetTimelineMetaDataById(string timelineId)
        {
            var data = LoadTimelineDataById(timelineId);
            if (data == null) return null;

            var state = _project?.TimelinesStates?.SingleOrDefault(s => s.Id == data.TimelineStateId);
            var stateName = state?.Title;

            return new TimelineMetaData(id: data.Id, title: data.Title, stateId: data.TimelineStateId, stateName: stateName ?? $"unknown StateId {data.TimelineStateId}", data.DurationMs);
        }

        /// <summary>
        ///  if there are no actual timeline files but old timeline files, convert them to the new format
        /// </summary>
        private void ConvertOldFilenamesIfNeeded(bool deleteOldFiles)
        {
            if (!TimelineRawFilenames.Any())
            {
                var oldFileNames = TimelineFilenamesOld.ToArray();
                foreach (var oldFileName in oldFileNames)
                {
                    var data = LoadTimelineDataByFilename(oldFileName);
                    if (data != null && data.Id == null)
                    {
                        data.Title = Path.GetFileNameWithoutExtension(oldFileName);
                        data.Id = System.Guid.NewGuid().ToString();
                        SaveTimelineData(data);
                        if (deleteOldFiles)
                            File.Delete(oldFileName);
                    }
                }
            }
        }
    }
}

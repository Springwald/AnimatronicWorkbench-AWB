// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.LoadNSave.TimelineLoadNSave;
using Awb.Core.Sounds;
using Awb.Core.Timelines;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Awb.Core.Services
{
    public class TimelineDataServiceByJsonFiles : ITimelineDataService
    {
        private readonly string _jsonFilesPath;
        private readonly Sound[] _projectSounds;
        private TimelineMetaDataService? _timelineMetaDataService;

        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
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

        public ITimelineMetaDataService TimelineMetaDataService
        {
            get
            {
                _timelineMetaDataService ??= new TimelineMetaDataService(this, _projectSounds);
                return _timelineMetaDataService;
            }
        }

        public IEnumerable<string> TimelineIds => Directory.GetFiles(_jsonFilesPath, "*.awbt").Select(f => Path.GetFileNameWithoutExtension(f));


        public IEnumerable<TimelineData> GetAllTimelinesDatas()
        {
            foreach(var id in TimelineIds)
            {
                var data = GetTimelineData(id);
                if (data == null)
                {                   
                    throw new FileNotFoundException($"Timeline data for ID '{id}' not found in '{_jsonFilesPath}'.");
                }
                else
                {
                    yield return data;
                }
            }
        }

        public TimelineDataServiceByJsonFiles(string jsonFilesPath, Sound[] projectSounds)
        {
            _jsonFilesPath = jsonFilesPath;
            _projectSounds = projectSounds;
            ConvertOldFilenamesIfNeeded(deleteOldFiles: true);
        }


        public TimelineData? GetTimelineData(string timelineId)
        {
            var filename = GetTimelineFilenameById(timelineId);
            return LoadTimelineDataByFilename(filename, timelineId);
        }



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

        public bool Exists(string timelineId) => File.Exists(GetTimelineFilenameById(timelineId));


        private IEnumerable<string> TimelineFilenamesOld => Directory.GetFiles(_jsonFilesPath, "*.awbtl");
        private IEnumerable<string> TimelineRawFilenames => Directory.GetFiles(_jsonFilesPath, "*.awbt");

        private string GetTimelineFilenameById(string timelineId) => Path.Combine(_jsonFilesPath, $"{timelineId}.awbt");

        private TimelineData? LoadTimelineDataByFilename(string filename, string timelineId)
        {
            if (!File.Exists(filename)) return null;
            var jsonString = System.IO.File.ReadAllText(filename);
            var saveFormat = JsonSerializer.Deserialize<TimelineSaveFormat>(jsonString, _jsonOptions);
            if (saveFormat == null) return null;
            var timelineData = TimelineSaveFormat.ToTimelineData(saveFormat, timelineId);
            return timelineData;
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
                    var newId = System.Guid.NewGuid().ToString();
                    var data = LoadTimelineDataByFilename(oldFileName, newId);
                    if (data != null)
                    {
                        data.Title = Path.GetFileNameWithoutExtension(oldFileName);
                        SaveTimelineData(data);
                        if (deleteOldFiles)
                            File.Delete(oldFileName);
                    }
                }
            }
        }

      
    }
}

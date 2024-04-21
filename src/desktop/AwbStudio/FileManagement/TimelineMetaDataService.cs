// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AwbStudio.FileManagement
{
    public interface ITimelineMetaDataService
    {
        int GetDurationMs(string timelineId);
        bool ExistsTimeline(string timelineId);
        void ClearCache(string timelineId);
        TimelineMetaData GetMetaData(string timelineId);
        TimelineMetaData[] GetAllMetaData();
    }

    public class TimelineMetaDataService : ITimelineMetaDataService
    {
        private readonly TimelineFileManager _timelineFileManager;

        private Dictionary<string, int> _durationCache = new Dictionary<string, int>();
        private TimelineMetaData[]? _allMetaDataCache = null;

        public TimelineMetaDataService(TimelineFileManager timelineFileManager)
        {
            _timelineFileManager = timelineFileManager;
        }

        public void ClearCache(string timelineId)
        {
            if (_durationCache.ContainsKey(timelineId)) _durationCache.Remove(timelineId);
            _allMetaDataCache = null;   
        }

        public bool ExistsTimeline(string timelineId)
        {
            var filename = _timelineFileManager.GetTimelineFilenameById(timelineId);
            return File.Exists(filename);
        }

        public TimelineMetaData[] GetAllMetaData()
        {
            if (_allMetaDataCache != null) return _allMetaDataCache;    

            var timelineFilenames = _timelineFileManager.TimelineFilenames;
            var result = new List<TimelineMetaData>();  
            foreach (var timelineFilename in timelineFilenames)
            {
                var metaData = _timelineFileManager.GetTimelineMetaData(timelineFilename);
                if (metaData != null) result.Add(metaData);
            }
            _allMetaDataCache = result.OrderBy(t => t.StateName).ThenBy(t => t.Title).ToArray();
            return _allMetaDataCache;
        }

        public int GetDurationMs(string timelineId)
        {
            if (_durationCache.ContainsKey(timelineId)) return _durationCache[timelineId];
            var metaData  = GetMetaData(timelineId);
            _durationCache[timelineId] = metaData.DurationMs;
            return metaData.DurationMs;
        }

        public TimelineMetaData GetMetaData(string timelineId)
        {
            var filename = _timelineFileManager.GetTimelineFilenameById(timelineId);
            return _timelineFileManager.GetTimelineMetaData(filename) ?? throw new  FileNotFoundException($"Timeline '{filename}' not found!");
        }
    }
}

// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Services;
using Awb.Core.Sounds;

namespace Awb.Core.Timelines
{
    public interface ITimelineMetaDataService
    {
        int GetDurationMs(TimelineData timelineData);
        int GetDurationMs(string timelineId);
        bool ExistsTimeline(string timelineId);
        void ClearCache(string timelineId);
        TimelineMetaData GetMetaData(string timelineId);
        TimelineMetaData[] GetAllMetaData();
    }

    public class TimelineMetaDataService : ITimelineMetaDataService
    {
        private readonly ITimelineDataService _timelineDataService;
        private readonly Sound[] _projectSounds;
        private Dictionary<string, int> _durationCache = new Dictionary<string, int>();
        private TimelineMetaData[]? _allMetaDataCache = null;

        public TimelineMetaDataService(ITimelineDataService timelineDataService, Sound[] projectSounds)
        {
            _timelineDataService = timelineDataService;
            _projectSounds = projectSounds;
        }

        public void ClearCache(string timelineId)
        {
            if (_durationCache.ContainsKey(timelineId)) _durationCache.Remove(timelineId);
            _allMetaDataCache = null;
        }

        public bool ExistsTimeline(string timelineId) => _timelineDataService.Exists(timelineId);

        public TimelineMetaData[] GetAllMetaData()
        {
            if (_allMetaDataCache != null) return _allMetaDataCache;

            var timelineIds = _timelineDataService.TimelineIds;
            var result = new List<TimelineMetaData>();
            foreach (var timelineId in timelineIds)
            {
                var metaData = GetMetaData(timelineId);
                if (metaData != null) result.Add(metaData);
            }
            _allMetaDataCache = result.OrderBy(t => t.Title).ToArray();
            return _allMetaDataCache;
        }

        public int GetDurationMs(TimelineData timelineData)
        {
            if (_durationCache.ContainsKey(timelineData.Id)) return _durationCache[timelineData.Id];
            var metaData = GetMetaData(timelineData);
            _durationCache[timelineData.Id] = metaData.DurationMs;
            return metaData.DurationMs;
        }
        public int GetDurationMs(string timelineId)
        {
            if (_durationCache.ContainsKey(timelineId)) return _durationCache[timelineId];
            var metaData = GetMetaData(timelineId);
            _durationCache[timelineId] = metaData.DurationMs;
            return metaData.DurationMs;
        }

        public TimelineMetaData GetMetaData(string timelineId)
        {
            var data = _timelineDataService.GetTimelineData(timelineId);
            if (data == null) throw new Exception($"Timeline with id {timelineId} not found");
            return GetMetaData(data);
        }

        private TimelineMetaData GetMetaData(TimelineData timelineData)
        {
            if (timelineData == null) throw new ArgumentNullException(nameof(timelineData), "Timeline data cannot be null");
            return new TimelineMetaData(id: timelineData.Id, title: timelineData.Title, stateId: timelineData.TimelineStateId, timelineData.GetDurationMs(_projectSounds, _timelineDataService));
        }
    }
}

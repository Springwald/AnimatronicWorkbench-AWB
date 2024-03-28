// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

namespace Awb.Core.Timelines
{
    public class TimelineData
    {
        public int TimelineStateId { get; set; }

        public string Title { get; set; } = "no name";

        /// <summary>
        /// The content of the timeline has changed
        /// </summary>
        public EventHandler OnContentChanged;

        /// <summary>
        /// What is the duration of the timeline filled with points?
        /// </summary>
        public int DurationMs => AllPoints?.Any() == true ? AllPoints.Max(p => p.TimeMs) : 0;

        public IEnumerable<TimelinePoint> AllPoints
        {
            get
            {
                foreach (var point in ServoPoints) yield return point;
                foreach (var point in SoundPoints) yield return point;
            }
        }

        /// <summary>
        /// All servo values changes of the timeline are stores as single points
        /// </summary>
        public List<ServoPoint> ServoPoints { get; set; }

        /// <summary>
        /// All sound events of the timeline are stores as single points
        /// </summary>
        public List<SoundPoint> SoundPoints { get; set; }
        public int LatestPointMs => AllPoints.Max(p => p.TimeMs);

        public TimelineData(List<ServoPoint> servoPoints, List<SoundPoint> soundPoints, int timelineStateId)
        {
            TimelineStateId = timelineStateId;
            ServoPoints = servoPoints;
            SoundPoints = soundPoints;
        }
        public void SetContentChanged()
        {
            OnContentChanged?.Invoke(this, new EventArgs());
        }   
    }
}

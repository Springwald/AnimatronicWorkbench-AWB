// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

namespace AwbStudio.TimelineEditing
{
    public class TimelineControllerPlayViewPos
    {
        /// <summary>
        /// The range in which the PlayPos can be moved by the controller
        /// </summary>
        public int PageWidthMs { get; set; } = 10 * 1000; // 10 seconds

        /// <summary>
        /// The PlayPos in ms, which represents the start of the ScrollPageWidthMs
        /// </summary>
        public int OriginMs { get; set; }

        /// <summary>
        ///  a value between 0 and PageWidthMs, which represents the position of the PlayPos in the PageWidthMs
        /// </summary>
        public int PlayPosRelativeToOriginMs { get; set; }

        /// <summary>
        ///  The absolute position of the PlayPos set by in ms
        /// </summary>
        public int PlayPosAbsoluteMs => OriginMs + PlayPosRelativeToOriginMs;

        public void SetPlayPosFromTimelineControl(int playPosMs)
        {
            OriginMs = playPosMs - PlayPosRelativeToOriginMs;
        }

        public void SetPositionFromValueInPercent(double valueInPercent)
        {
            PlayPosRelativeToOriginMs = (int)(PageWidthMs * valueInPercent / 100.0);
        }
    }
}

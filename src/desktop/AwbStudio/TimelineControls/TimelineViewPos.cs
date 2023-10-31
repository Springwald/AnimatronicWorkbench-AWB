// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Player;
using Awb.Core.Timelines;
using System;

namespace AwbStudio.TimelineControls
{
    public class TimelineViewPos
    {
        private int _displayMs = 10 * 1000; // default is 10 seconds
        private int _scrollOffsetMs = 0;
        private int _posSelectorManualMs = 0;

        /// <summary>
        /// Is fired, when the timeline position or view has changed
        /// </summary>
        public EventHandler? Changed;

        /// <summary>
        /// How many seconds are displayed in the timeline
        /// </summary>
        public int DisplayMs
        {
            get => _displayMs;
            set
            {
                if (_displayMs == value) return;
                _displayMs = value;
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// The left offset of the timeline in seconds
        /// </summary>
        public int ScrollOffsetMs
        {
            get => _scrollOffsetMs;
            set
            {
                if (_scrollOffsetMs == value) return;
                _scrollOffsetMs = value;
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// The position of the midi-input controller fader in milli seconds
        /// </summary>
        public int PosSelectorManualMs
        {
            get => _posSelectorManualMs;
            set
            {
                if (_posSelectorManualMs == value) return;
                _posSelectorManualMs = value;
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <returns>true:value has changed; false:no change in the value</returns>
        public bool SetPosSelectorManualMsByPercent(double percent)
        {
            var newMs = ((int)((this.DisplayMs / 100.0 * percent) / TimelinePlayer.PlayPosSnapMs) * TimelinePlayer.PlayPosSnapMs);
            if (newMs == this.PosSelectorManualMs) return false;
            this.PosSelectorManualMs = ((int)((this.DisplayMs / 100.0 * percent) / TimelinePlayer.PlayPosSnapMs) * TimelinePlayer.PlayPosSnapMs);
            return true;
        }

        public double GetPosSelectorPercent()
            => this.PosSelectorManualMs * 100.0 / this.DisplayMs;

        /// <summary>
        /// The x paint position on the screen
        /// </summary>
        /// <param name="ms">milliseconds on the play timeline</param>
        /// <param name="controlWidth">the width of the painting control</param>
        /// <param name="timelineData">the data of the actual timeline</param>
        /// <returns></returns>
        public double GetXPos(int ms, double controlWidth, TimelineData timelineData) =>
            timelineData == null ? 0 :
            ((double)(ms - ScrollOffsetMs) / DisplayMs) * controlWidth;

        /// <summary>
        /// The x paint position on the screen
        /// </summary>
        /// <param name="ms">milliseconds on the play timeline</param>
        /// <param name="controlWidth">the width of the painting control</param>
        /// <returns></returns>
        public double GetXPos(int ms, double controlWidth) =>
            ((double)(ms - ScrollOffsetMs) / DisplayMs) * controlWidth;
    }
}

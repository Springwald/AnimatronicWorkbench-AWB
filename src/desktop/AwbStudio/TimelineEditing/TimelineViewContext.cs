// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Timelines;
using System;

namespace AwbStudio.TimelineEditing
{
    public class TimelineViewContext
    {
        private int _durationMs = 20 * 1000;

        /// <summary>
        ///  The index of the active actuator bank
        /// </summary>
        private int _bankIndex;

        /// <summary>
        /// Is fired, when the timeline position or view has changed
        /// </summary>
        public EventHandler? Changed;
        private double _pixelPerMs;

        public double PixelPerMs
        {
            get => _pixelPerMs;
            set
            {
                if (_pixelPerMs == value) return;
                _pixelPerMs = value;
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        public int DurationMs
        {
            get => _durationMs;
            set
            {
                if (_durationMs == value) return;
                _durationMs = value;
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// How many items are grouped in one actuator bank
        /// </summary>
        public int ItemsPerBank { get; } = 8;

        /// <summary>
        ///  The index of the active actuator bank
        /// </summary>
        public int BankIndex
        {
            get => _bankIndex;
            set
            {
                if (_bankIndex == value) return;
                _bankIndex = value;
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        public int FirstBankItemNo => BankIndex * ItemsPerBank + 1; // base 1
        public int LastBankItemNo => FirstBankItemNo + ItemsPerBank - 1; // base 1


        public double GetXPos(int timeMs, TimelineData? timelineData) => timelineData == null ? 0 : GetXPos(timeMs);
        public double GetXPos(int timeMs) => timeMs * PixelPerMs;
    }
}

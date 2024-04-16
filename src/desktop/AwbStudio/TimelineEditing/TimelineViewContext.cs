// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.ActuatorsAndObjects;
using Awb.Core.Timelines;
using System;

namespace AwbStudio.TimelineEditing
{
    public class TimelineViewContext
    {
        /// <summary>
        ///  The index of the active actuator bank
        /// </summary>
        private int _bankIndex;
        private double _pixelPerMs;
        private double _scrollPositionPx;
        private int _durationMs = 20 * 1000;
        private IAwbObject? _actualFocusObject;

        /// <summary>
        /// Is fired, when the timeline position or view has changed
        /// </summary>
        public EventHandler<ViewContextChangedEventArgs>? Changed;

        public void FocusObjectValueChanged()
        {
            Changed?.Invoke(this, new ViewContextChangedEventArgs(ViewContextChangedEventArgs.ChangeTypes.FocusObjectValue));
        }   

        public IAwbObject? ActualFocusObject
        {
            get => _actualFocusObject;
            set
            {
                if (_actualFocusObject?.Equals(value) == true) return;
                _actualFocusObject = value;
                Changed?.Invoke(this, new ViewContextChangedEventArgs(ViewContextChangedEventArgs.ChangeTypes.FocusObject));
            }
        }

        public double PixelPerMs
        {
            get => _pixelPerMs;
            set
            {
                if (_pixelPerMs.Equals(value)) return;
                _pixelPerMs = value;
                Changed?.Invoke(this, new ViewContextChangedEventArgs(ViewContextChangedEventArgs.ChangeTypes.PixelPerMs));
            }
        }

        public int DurationMs
        {
            get => _durationMs;
            set
            {
                if (_durationMs.Equals(value)) return;
                _durationMs = value;
                Changed?.Invoke(this, new ViewContextChangedEventArgs(ViewContextChangedEventArgs.ChangeTypes.Duration));
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
                if (_bankIndex.Equals(value)) return;
                _bankIndex = value;
                Changed?.Invoke(this, new ViewContextChangedEventArgs(ViewContextChangedEventArgs.ChangeTypes.BankIndex));
            }
        }

        /// <summary>
        /// What is the current horizontal scroll position in the timeline
        /// </summary>
        public double ScrollPositionPx
        {
            get => _scrollPositionPx;
            set
            {
                if (_scrollPositionPx.Equals(value)) return;
                _scrollPositionPx = value;
                Changed?.Invoke(this, new ViewContextChangedEventArgs(ViewContextChangedEventArgs.ChangeTypes.Scroll));
            }
        }

        public int FirstBankItemNo => BankIndex * ItemsPerBank + 1; // base 1
        public int LastBankItemNo => FirstBankItemNo + ItemsPerBank - 1; // base 1
        public double GetXPos(int timeMs, TimelineData? timelineData) => timelineData == null ? 0 : GetXPos(timeMs);
        public double GetXPos(int timeMs) => timeMs * PixelPerMs;
    }
}

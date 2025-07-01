// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

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

        /// <summary>
        /// How many pixels are needed to represent one ms in the timeline x -axis
        /// </summary>
        private double _pixelPerMs;

        /// <summary>
        /// How many pixels are scrolled in the timeline x-view
        /// </summary>
        private double _scrollPositionPx;

        /// <summary>
        /// The total duration of the timeline in ms, initially 20s
        /// </summary>
        private int _durationMs = 20 * 1000;

        /// <summary>
        /// The actual focus object if selected
        /// </summary>
        private IAwbObject? _actualFocusObject;

        /// <summary>
        /// If a selection is active, this is the start of the selection in ms
        /// </summary>
        private int? _selectionStartMs;

        /// <summary>
        /// If a selection is active, this is the end of the selection in ms
        /// </summary>
        private int? _selectionEndMs;

        /// <summary>
        /// Is fired, when the timeline position or view has changed
        /// </summary>
        public EventHandler<ViewContextChangedEventArgs>? Changed;

        /// <summary>
        /// If true, no actuator scrolling into view when ActualFocusObject changes is wanted at the moment
        /// </summary>
        public volatile bool PreventAllActuatorScrolling;

        public void FocusObjectValueChanged(object sender)
        {
            Changed?.Invoke(sender, new ViewContextChangedEventArgs(ViewContextChangedEventArgs.ChangeTypes.FocusObjectValue));
        }

        /// <summary>
        ///  The actual focus object if selected
        /// </summary>
        public IAwbObject? ActualFocusObject
        {
            get => _actualFocusObject;
            set
            {
                //if (_actualFocusObject?.Equals(value) == true) return;
                _actualFocusObject = value;
                Changed?.Invoke(this, new ViewContextChangedEventArgs(ViewContextChangedEventArgs.ChangeTypes.FocusObject));
            }
        }

        /// <summary>
        /// How many pixels are needed to represent one ms in the timeline x -axis
        /// </summary>
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

        /// <summary>
        /// The total duration of the timeline in ms, initially 20s
        /// </summary>
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
        /// The duration of the timeline in ms, but extended so that the timeline can grow beyond the duration of the last keaframe
        /// </summary>
        public int DurationMsExtended => DurationMs + 5000; // 5000ms extra

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

        /// <summary>
        /// When a selection is active, this is the start of the selection in ms
        /// </summary>
        public int? SelectionStartMs
        {
            get => _selectionStartMs;
            set
            {
                if (_selectionStartMs.Equals(value)) return;
                _selectionStartMs = value;
                Changed?.Invoke(this, new ViewContextChangedEventArgs(ViewContextChangedEventArgs.ChangeTypes.Selection));
            }
        }

        /// <summary>
        /// When a selection is active, this is the end of the selection in ms
        /// </summary>
        public int? SelectionEndMs
        {
            get => _selectionEndMs;
            set
            {
                if (_selectionEndMs.Equals(value)) return;
                _selectionEndMs = value;
                Changed?.Invoke(this, new ViewContextChangedEventArgs(ViewContextChangedEventArgs.ChangeTypes.Selection));
            }
        }

        /// <summary>
        /// For usage of a midi controller: this is the actuator item number of the first item in the bank
        /// </summary>
        public int FirstBankItemNo => BankIndex * ItemsPerBank + 1; // base 1

        /// <summary>
        /// For usage of a midi controller: this is the actuator item number of the last item in the bank
        /// </summary>
        public int LastBankItemNo => FirstBankItemNo + ItemsPerBank - 1; // base 1

        /// <summary>
        ///  Calculates the x position of a given time in the timeline
        /// </summary>
        /// <returns></returns>
        public double GetXPos(int timeMs, TimelineData? timelineData) => timelineData == null ? 0 : GetXPos(timeMs);

        /// <summary>
        ///  calculates the x position of a given time in the timeline
        /// </summary>
        public double GetXPos(int timeMs) => timeMs * PixelPerMs;
    }
}

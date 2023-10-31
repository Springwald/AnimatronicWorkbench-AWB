// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Player;
using Awb.Core.Services;
using Awb.Core.Timelines;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AwbStudio.TimelineControls
{
    public partial class TimelineViewer : UserControl
    {
        private int _playPosMs = 0;
        private bool _wasPlaying = false;
        private readonly Brush _gridLineBrush = new SolidColorBrush(Color.FromRgb(60, 60, 100));
        private TimelineData? _timelineData;

        private IActuatorsService? _actuatorsService;
        private TimelinePlayer? _timelinePlayer;
        private TimelineViewPos _viewPos;

        private void PlayerStateChanged(object? sender, PlayStateEventArgs e)
        {
            this._playPosMs = e.PositionMs;
            var scrollingChanged = false;

            switch (e.PlayState)
            {
                case TimelinePlayer.PlayStates.Nothing:
                    if (_wasPlaying)
                    {
                        scrollingChanged = SyncScrollOffsetToNewPlayPos(e.PositionMs, snapToGrid: true);
                        _wasPlaying = false;
                    }
                    break;

                case TimelinePlayer.PlayStates.Playing:
                    scrollingChanged = SyncScrollOffsetToNewPlayPos(e.PositionMs, snapToGrid: false);
                    _wasPlaying = true;
                    break;

                default: throw new ArgumentOutOfRangeException($"{nameof(e.PlayState)}:{e.PlayState.ToString()}");
            }

            if (scrollingChanged)
            {
                MyInvoker.Invoke(new Action(() => this.PaintTimeLine()));
            }
            else
            {
                MyInvoker.Invoke(new Action(() => this.PaintPlayPos(_timelineData)));
            }
        }

        /// <summary>
        /// The actual view and scroll position of the timeline
        /// </summary>
        public TimelineViewPos ViewPos
        {
            set
            {
                if (value != null)
                {
                    if (_viewPos != null) throw new Exception("ViewPos.Changed is already set");
                    _viewPos = value;
                    _viewPos.Changed += this.OnViewPosChanged;
                    ServoValueViewer.ViewPos = value;
                    this.OnViewPosChanged(this, EventArgs.Empty);
                }
            }
            get => _viewPos;
        }


        /// <summary>
        /// The timeline data to be displayed
        /// </summary>
        public TimelineData? TimelineData
        {
            get { return _timelineData; }
            set
            {
                _timelineData = value;
                ServoValueViewer.TimelineData = value;
                SyncScrollOffsetToNewPlayPos(0, snapToGrid: true);
                PaintTimeLine();
                PaintPlayPos(_timelineData);
            }
        }

        /// <summary>
        /// The service to get the actuators
        /// </summary>
        public IActuatorsService? ActuatorsService
        {
            get => _actuatorsService; set
            {
                _actuatorsService = value;
                ServoValueViewer.ActuatorsService = value;
            }
        }

        /// <summary>
        /// The timeline player to control the timeline playback
        /// </summary>
        public TimelinePlayer? Timelineplayer
        {
            get => _timelinePlayer;
            set
            {
                _timelinePlayer = value;
                if (_timelinePlayer != null) _timelinePlayer.OnPlayStateChanged += this.PlayerStateChanged;
            }
        }


        /// <summary>
        /// set the scroll offset in a way, that the manual playPos is always at the position chosen in the hardware controller 
        /// </summary>
        /// <returns>true, when changed</returns>
        public bool SyncScrollOffsetToNewPlayPos(int newPlayPosMs, bool snapToGrid)
        {
            if (this._timelinePlayer != null)
            {
                if (snapToGrid)
                {
                    this.ViewPos.ScrollOffsetMs = ((newPlayPosMs - this.ViewPos.PosSelectorManualMs) / TimelinePlayer.PlayPosSnapMs) * TimelinePlayer.PlayPosSnapMs;
                }
                else
                {
                    this.ViewPos.ScrollOffsetMs = newPlayPosMs - this.ViewPos.PosSelectorManualMs;
                }
                return true;
            }
            return false;
        }

        public TimelineViewer()
        {
            InitializeComponent();

            //MyViewModel vm = new MyViewModel();
            Loaded += TimelineViewer_Loaded;
            SizeChanged += TimelineViewer_SizeChanged;
        }

        public void PaintTimeLine()
        {
            if (ViewPos == null) return;

            const double _paintMarginTopBottom = 30;
            double height = GridTimeline.ActualHeight;
            double width = GridTimeline.ActualWidth;

            if (height < 100 || width < 100) return;

            //r servoIds = _timelineData?.ServoPoints?.OfType<ServoPoint>().Select(p => p.ServoId).Distinct().ToArray() ?? Array.Empty<string>();

            double diagramHeight = height - _paintMarginTopBottom * 2;

            // update the data optical grid lines
            OpticalGrid.Children.Clear();
            var duration = ViewPos.DisplayMs;
            const int STEP = 1000;
            for (int msRaw = 0; msRaw < duration + ViewPos.DisplayMs; msRaw += STEP)
            {
                int ms = msRaw - ViewPos.ScrollOffsetMs;
                var x = ms * width / duration;
                if (x > 0 && x < width)
                {
                    OpticalGrid.Children.Add(new Line { X1 = x, X2 = x, Y1 = _paintMarginTopBottom, Y2 = height - _paintMarginTopBottom, Stroke = _gridLineBrush });
                    OpticalGrid.Children.Add(new Label { Content = ((ms + ViewPos.ScrollOffsetMs) / STEP).ToString(), BorderThickness = new Thickness(left: x, top: height - 30, right: 0, bottom: 0) });
                }
            }

            ServoValueViewer.PaintServoValues();

            // update the play position
            PaintPlayPos(_timelineData);
        }

        private void OnViewPosChanged(object? sender, EventArgs e)
        {
            if (_timelineData == null) return;

            // update the scrollbar
            MyInvoker.Invoke(() =>
            {
                TimelineScrollbar.Value = ViewPos.GetPosSelectorPercent();
                PaintTimeLine();  // update the timeline
            });
        }

        private void TimelineViewer_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= TimelineViewer_Loaded;
            this.PaintTimeLine();
            Unloaded += TimelineViewer_Unloaded;
        }

        private void TimelineViewer_Unloaded(object sender, RoutedEventArgs e)
        {
            Unloaded -= TimelineViewer_Unloaded;
            if (_timelinePlayer != null) _timelinePlayer.OnPlayStateChanged -= this.PlayerStateChanged;
        }


        private void TimelineViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            PaintTimeLine();
            PaintPlayPos(_timelineData);
        }

        private void PaintPlayPos(TimelineData? timeline)
        {
            // draw the manual midi controller play position as triangle at the bottom
            var x = ((double)(ViewPos.PosSelectorManualMs) / ViewPos.DisplayMs) * this.ActualWidth;
            ManualPlayPosAbsolute.Margin = new Thickness(x - ManualPlayPosAbsolute.ActualWidth / 2, 0, 0, 0);

            // draw the play position as a vertical line
            if (timeline == null || _playPosMs < 0 || _playPosMs > ViewPos.ScrollOffsetMs + ViewPos.DisplayMs)
            {
                PlayPosLine.Visibility = Visibility.Hidden;
            }
            else
            {
                x = this.ViewPos.GetXPos(ms: _playPosMs, controlWidth: this.ActualWidth);
                PlayPosLine.X1 = x;
                PlayPosLine.X2 = x;
                PlayPosLine.Y1 = 0;
                PlayPosLine.Y2 = this.ActualHeight;
                PlayPosLine.Visibility = Visibility.Visible;
            }
        }

        private void TimelineScrollbar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ViewPos.SetPosSelectorManualMsByPercent(e.NewValue))
            {
                MyInvoker.Invoke(() =>
                {
                    PaintTimeLine(); // update the timeline
                });
            }
        }
    }

}
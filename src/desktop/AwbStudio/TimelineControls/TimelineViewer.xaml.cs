﻿// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
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
    public partial class TimelineViewer : UserControl, ITimelineControl
    {
        private const int ItemsPerBank = 8;

        private int _actualBankIndex = 0;
        private bool _wasPlaying = false;
        private readonly Brush _gridLineBrush = new SolidColorBrush(Color.FromRgb(60, 60, 100));
        private TimelineData? _timelineData;
        private int _lastBankIndex = -1;
        private IActuatorsService? _actuatorsService;
        private TimelineCaptions _timelineCaptions;
        private TimelineViewContext _viewContext;
        private PlayPosSynchronizer _playPosSynchronizer;
        private bool _isInitialized;
        private TimelinePlayer? _timelinePlayer;


        private ITimelineControl[] _subTimelineViewerControls;

        /// <summary>
        /// The timeline player to control the timeline playback
        /// </summary>
        public TimelinePlayer? Timelineplayer
        {
            get => _timelinePlayer;
            set
            {
                _timelinePlayer = value;
                if (_timelinePlayer != null)
                {

                }
            }
        }

        public TimelineViewer()
        {
            InitializeComponent();

            //MyViewModel vm = new MyViewModel();
            SizeChanged += TimelineViewer_SizeChanged;

            // set up the sub-timeline-viewers
            _subTimelineViewerControls = new ITimelineControl[] { CaptionsViewer, ServoValueViewer, SoundValueViewer };
        }

        public void Init(TimelineViewContext viewContext, TimelineCaptions timelineCaptions, PlayPosSynchronizer playPosSynchronizer, IActuatorsService actuatorsService)
        {
            _viewContext = viewContext;
            _timelineCaptions = timelineCaptions;
            _playPosSynchronizer = playPosSynchronizer;
            _actuatorsService = actuatorsService;

            _viewContext.Changed += OnViewContextChanged;
            _playPosSynchronizer.OnPlayPosChanged += (sender, e) => MyInvoker.Invoke(new Action(() => { PaintPlayPos(); }));

            foreach (var subTimelineViewerControl in _subTimelineViewerControls)
                subTimelineViewerControl.Init(viewContext, timelineCaptions, playPosSynchronizer, actuatorsService);

            _isInitialized = true;
        }

        public void TimelineDataLoaded(TimelineData timelineData)
        {
            if (!_isInitialized) throw new InvalidOperationException(Name + " not initialized");

            _timelineData = timelineData;

            foreach (var subTimelineViewerControl in _subTimelineViewerControls)
                subTimelineViewerControl.TimelineDataLoaded(timelineData);

            PaintGrid();
            PaintPlayPos();
        }

        public void PaintGrid()
        {
            if (!_isInitialized) throw new InvalidOperationException(Name + " not initialized");

            const double _paintMarginTopBottom = 30;
            double height = GridTimeline.ActualHeight;
            double width = GridTimeline.ActualWidth;

            if (height < 100 || width < 100) return;

            //r servoIds = _timelineData?.ServoPoints?.OfType<ServoPoint>().Select(p => p.ServoId).Distinct().ToArray() ?? Array.Empty<string>();

            double diagramHeight = height - _paintMarginTopBottom * 2;

            // update the data optical grid lines
            OpticalGrid.Children.Clear();
            var duration = _viewContext.DurationMs;
            const int STEP = 1000;
            for (int ms = 0; ms < duration; ms += STEP)
            {
                var x = _viewContext.GetXPos(ms);
                if (x > 0 && x < width)
                {
                    OpticalGrid.Children.Add(new Line { X1 = x, X2 = x, Y1 = _paintMarginTopBottom, Y2 = height - _paintMarginTopBottom, Stroke = _gridLineBrush });
                    OpticalGrid.Children.Add(new Label { Content = ((ms) / STEP).ToString(), BorderThickness = new Thickness(left: x, top: height - 30, right: 0, bottom: 0) });
                }
            }
        }

        private void PaintPlayPos()
        {
            if (!_isInitialized) throw new InvalidOperationException(Name + " not initialized");

            // draw the manual midi controller play position as triangle at the bottom
            var x = _viewContext.GetXPos(_playPosSynchronizer.PlayPosMs);
            LabelManualPlayPosAbsolute.Margin = new Thickness(x - LabelManualPlayPosAbsolute.FontSize, 0, 0, 0);
            LabelManualPlayPosAbsolute.Content = $"🔺 {_playPosSynchronizer.PlayPosMs}ms";

            // draw the play position as a vertical line
            var playPosMs = _playPosSynchronizer.PlayPosMs;
            if (_timelineData == null || playPosMs < 0 || playPosMs > _viewContext.DurationMs)
            {
                PlayPosLine.Visibility = Visibility.Hidden;
            }
            else
            {
                x = this._viewContext.GetXPos(timeMs: playPosMs);
                PlayPosLine.X1 = x;
                PlayPosLine.X2 = x;
                PlayPosLine.Y1 = 0;
                PlayPosLine.Y2 = this.ActualHeight;
                PlayPosLine.Visibility = Visibility.Visible;
            }
        }



        private void OnViewContextChanged(object? sender, EventArgs e)
        {
            if (_timelineData == null) return;

            if (_lastBankIndex != _viewContext.BankIndex && _actuatorsService != null)
            {
                _lastBankIndex = _viewContext.BankIndex;
                MyInvoker.Invoke(new Action(() =>
                {
                    var bankStartItemNo = _viewContext.BankIndex * _viewContext.ItemsPerBank + 1; // base 1
                    labelBankNo.Content = $"Bank {_viewContext.BankIndex + 1} [{bankStartItemNo}-{Math.Min(_actuatorsService.AllIds.Length, bankStartItemNo + _viewContext.ItemsPerBank - 1)}]";
                }));
            }

            var newWidth = this._viewContext.PixelPerMs * this._viewContext.DurationMs;

            // update the scrollbar
            MyInvoker.Invoke(() =>
            {
                this.Width = newWidth;
                PaintGrid();  // update the timeline
            });
        }

        private void TimelineViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!_isInitialized) return;
            PaintGrid();
        }
    }
}
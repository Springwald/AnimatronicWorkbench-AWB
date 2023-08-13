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
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AwbStudio.TimelineControls
{
    public partial class TimelineViewer : UserControl
    {
        const double _paintMarginTopBottom = 30;

        private TimelineData? _timelineData;
        private readonly Brush _gridLineBrush = new SolidColorBrush(Color.FromRgb(60, 60, 100));
        private int _playPosMs = 0;

        private readonly TimelineViewPos _timelineViewPos = new TimelineViewPos();

        private TimelineCaptions? _timelineCaptions;

        public TimelineViewPos ViewPos => _timelineViewPos;

        public TimelineData? TimelineData
        {
            get { return _timelineData; }
            set
            {
                _timelineData = value;
                PaintTimeLine();
                PaintPlayPos(_timelineData);
            }
        }

        private IActuatorsService? _actuatorsService;
        public IActuatorsService? ActuatorsService
        {
            get => _actuatorsService; set
            {
                _actuatorsService = value;
                CalculateCaptions();
            }
        }

        public TimelinePlayer? _timelinePlayer;

        public TimelinePlayer? TimelinePlayer
        {
            get => _timelinePlayer;
            set
            {
                _timelinePlayer = value;
                if (_timelinePlayer != null) _timelinePlayer.OnPlayStateChanged += this.PlayerStateChanged;
            }
        }

        private void PlayerStateChanged(object? sender, PlayStateEventArgs e)
        {
            this._playPosMs = e.PositionMs;
            var scrollingChanged = false;

            switch (e.PlayState)
            {
                case TimelinePlayer.PlayStates.Nothing:
                    break;
                case TimelinePlayer.PlayStates.Playing:
                    if (e.PositionMs < this._timelineViewPos.ScrollOffsetMs)
                    {
                        this._timelineViewPos.ScrollOffsetMs = e.PositionMs;
                        scrollingChanged = true;
                    }
                    if (e.PositionMs > this._timelineViewPos.ScrollOffsetMs + this._timelineViewPos.DisplayMs)
                    {
                        this._timelineViewPos.ScrollOffsetMs = e.PositionMs - this._timelineViewPos.DisplayMs;
                        scrollingChanged = true;
                    }
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

        public TimelineViewer()
        {
            InitializeComponent();

            //MyViewModel vm = new MyViewModel();
            Loaded += TimelineViewer_Loaded;
            SizeChanged += TimelineViewer_SizeChanged;
        }

        public void PaintTimeLine()
        {
            if (this.ActualHeight < 100 || this.ActualWidth < 100) return;

            var servoIds = _timelineData?.ServoPoints?.OfType<ServoPoint>().Select(p => p.ServoId).Distinct().ToArray() ?? Array.Empty<string>();

            double diagramHeight = this.ActualHeight - _paintMarginTopBottom * 2;

            // Update the content points and lines
            // ToDo: cache and only update on changes; or: use model binding and auto update
            PanelLines.Children.Clear();
            GridDots.Children.Clear();
            foreach (var servoId in servoIds)
            {
                var caption = _timelineCaptions?.GetServoCaption(servoId) ?? new TimelineCaption { Color = new SolidColorBrush(Colors.White) };

                // Add polylines with points
                var pointsForThisServo = _timelineData?.ServoPoints.OfType<ServoPoint>().Where(p => p.ServoId == servoId).OrderBy(p => p.TimeMs).ToList() ?? new List<ServoPoint>();

                // add dots
                const int dotRadius = 3;
                const int dotWidth = dotRadius * 2;
                foreach (var point in pointsForThisServo)
                {
                    if (point.TimeMs >= _timelineViewPos.ScrollOffsetMs && point.TimeMs <= _timelineViewPos.DisplayMs + _timelineViewPos.ScrollOffsetMs) // is inside view
                    {
                        this.GridDots.Children.Add(new Ellipse
                        {
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Top,
                            Fill = caption.Color,
                            Stroke = caption.Color,
                            Height = dotWidth,
                            Width = dotWidth,
                            Margin = new Thickness { Left = GetXPos((int)point.TimeMs, _timelineData) - dotRadius, Top = this.ActualHeight - _paintMarginTopBottom - point.ValuePercent / 100.0 * diagramHeight - dotRadius }
                        });
                    }
                }

                //var firstPoint = pointsForThisServo.FirstOrDefault();
                //if (firstPoint != null && firstPoint.TimeMs > 0)
                //    pointsForThisServo.Insert(0, new ServoPoint(firstPoint.ServoId, firstPoint.ValuePercent, timeMs: 0));

                //var lastPoint = pointsForThisServo.LastOrDefault();
                //if (lastPoint != null && lastPoint.TimeMs <  _timelineViewPos.DisplayMs + _timelineViewPos.ScrollOffsetMs)
                //    pointsForThisServo.Add(new ServoPoint(lastPoint.ServoId, lastPoint.ValuePercent, timeMs: (int)RenderDurationMs));

                var points = new PointCollection(pointsForThisServo.Select(p => new Point { X = GetXPos((int)(p.TimeMs), _timelineData), Y = this.ActualHeight - _paintMarginTopBottom - p.ValuePercent / 100.0 * diagramHeight }));
                var line = new Polyline { Tag = ServoTag(servoId), Stroke = caption.Color, StrokeThickness = 1, Points = points };
                this.PanelLines.Children.Add(line);
            }

            // update the data optical grid lines
            OpticalGrid.Children.Clear();
            var duration = ViewPos.DisplayMs;
            for (int ms = 0; ms < duration; ms += 1000)
            {
                var x = ms * this.ActualWidth / duration;
                OpticalGrid.Children.Add(new Line { X1 = x, X2 = x, Y1 = _paintMarginTopBottom, Y2 = this.ActualHeight - _paintMarginTopBottom, Stroke = _gridLineBrush });
                OpticalGrid.Children.Add(new Label { Content = ((ms + ViewPos.ScrollOffsetMs) / 1000).ToString(), BorderThickness = new Thickness(left: x, top: this.ActualHeight - 30, right: 0, bottom: 0) });
            }
            foreach (var valuePercent in new[] { 0, 25, 50, 75, 100 })
            {
                var y = this.ActualHeight - _paintMarginTopBottom - valuePercent / 100.0 * diagramHeight;
                OpticalGrid.Children.Add(new Line { X1 = 0, X2 = this.ActualWidth, Y1 = y, Y2 = y, Stroke = _gridLineBrush });
            }

            // update the play position
            PaintPlayPos(_timelineData);
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

        private static string ServoTag(string servoId) => $"Servo {servoId}";

        private void CalculateCaptions()
        {
            if (ActuatorsService == null) throw new ArgumentNullException(nameof(ActuatorsService));

            _timelineCaptions = new TimelineCaptions();

            // Add servos
            int no = 1;
            foreach (var servo in ActuatorsService.Servos)
            {
                _timelineCaptions.AddServo(servo.Id, $"({no++}) {servo.Label}");
            }

            LineNames.Children.Clear();
            foreach (var caption in _timelineCaptions.Captions)
            {
                LineNames.Children.Add(new Label { Content = caption.Label, Foreground = caption.Color });
            }
        }



        private void PaintPlayPos(TimelineData? timeline)
        {
            if (timeline == null || _playPosMs < 0 || _playPosMs > _timelineViewPos.ScrollOffsetMs + _timelineViewPos.DisplayMs)
            {
                PlayPosLine.Visibility = Visibility.Hidden;
                return;
            }

            var x = GetXPos(_playPosMs, timeline);
            PlayPosLine.X1 = x;
            PlayPosLine.X2 = x;
            PlayPosLine.Y1 = 0;
            PlayPosLine.Y2 = this.ActualHeight;
            PlayPosLine.Visibility = Visibility.Visible;
        }


        private double GetXPos(int ms, TimelineData? timeline) =>
            (timeline == null)
            ? 0
            : ((double)(ms - _timelineViewPos.ScrollOffsetMs) / ViewPos.DisplayMs) * this.ActualWidth;
    }

}
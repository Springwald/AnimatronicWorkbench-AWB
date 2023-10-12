// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

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
    /// <summary>
    /// Interaction logic for ServoValueViewerControl.xaml
    /// </summary>
    public partial class ServoValueViewerControl : UserControl
    {
        private readonly Brush _gridLineBrush = new SolidColorBrush(Color.FromRgb(60, 60, 100));

        private TimelineCaptions? _timelineCaptions;
        private TimelineData? _timelineData;
        private TimelineViewPos? _viewPos;
        private IActuatorsService? _actuatorsService;

        public ServoValueViewerControl()
        {
            InitializeComponent();
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
                PaintServoValues();
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
                    this.OnViewPosChanged(this, EventArgs.Empty);
                }
            }
            get => _viewPos;
        }

        private void OnViewPosChanged(object? sender, EventArgs e)
        {
            PaintServoValues();
        }

        private void PaintServoValues()
        {
            const double _paintMarginTopBottom = 30;

            PanelLines.Children.Clear();
            GridDots.Children.Clear();

            if (_viewPos == null) return;
            if (_timelineData == null) return;
            
            double height = this.ActualHeight;
            double width = this.ActualWidth;

            if (height < 100 || width < 100) return;

            var servoIds = _timelineData?.ServoPoints?.OfType<ServoPoint>().Select(p => p.ServoId).Distinct().ToArray() ?? Array.Empty<string>();

            double diagramHeight = height - _paintMarginTopBottom * 2;

            // Update the content points and lines
            // ToDo: cache and only update on changes; or: use model binding and auto update
            
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
                    if (point.TimeMs >= ViewPos.ScrollOffsetMs && point.TimeMs <= ViewPos.DisplayMs + ViewPos.ScrollOffsetMs) // is inside view
                    {
                        this.GridDots.Children.Add(new Ellipse
                        {
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Top,
                            Fill = caption.Color,
                            Stroke = caption.Color,
                            Height = dotWidth,
                            Width = dotWidth,
                            Margin = new Thickness { Left = _viewPos.GetXPos(ms: (int)point.TimeMs, controlWidth: width, timelineData: _timelineData) - dotRadius, Top = height - _paintMarginTopBottom - point.ValuePercent / 100.0 * diagramHeight - dotRadius }
                        }) ;
                    }
                }

                var points = new PointCollection(pointsForThisServo.Select(p => new Point { X = _viewPos.GetXPos((int)(p.TimeMs), controlWidth: width, timelineData: _timelineData), Y = height - _paintMarginTopBottom - p.ValuePercent / 100.0 * diagramHeight }));
                var line = new Polyline { Tag = ServoTag(servoId), Stroke = caption.Color, StrokeThickness = 1, Points = points };
                this.PanelLines.Children.Add(line);
            }

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
            foreach (var valuePercent in new[] { 0, 25, 50, 75, 100 })
            {
                var y = height - _paintMarginTopBottom - valuePercent / 100.0 * diagramHeight;
                OpticalGrid.Children.Add(new Line { X1 = 0, X2 = width, Y1 = y, Y2 = y, Stroke = _gridLineBrush });
            }
        }

        private static string ServoTag(string servoId) => $"Servo {servoId}";

        private void CalculateCaptions()
        {
            if (_actuatorsService == null) throw new ArgumentNullException(nameof(ActuatorsService));

            _timelineCaptions = new TimelineCaptions();

            // Add servos
            int no = 1;
            foreach (var servo in _actuatorsService.Servos)
            {
                _timelineCaptions.AddServo(servo.Id, $"({no++}) {servo.Label}");
            }

            LineNames.Children.Clear();
            foreach (var caption in _timelineCaptions.Captions)
            {
                LineNames.Children.Add(new Label { Content = caption.Label, Foreground = caption.Color });
            }
        }

        //  rect.ToolTip = $"{servoValueName} {servoValueMsLeft}ms - {servoValueMsRight}ms ({servoValue.DurationMs}ms)";

    }
}

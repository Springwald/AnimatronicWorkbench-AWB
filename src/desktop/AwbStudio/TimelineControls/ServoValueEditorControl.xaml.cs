// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Actuators;
using Awb.Core.Player;
using Awb.Core.Services;
using Awb.Core.Timelines;
using AwbStudio.TimelineEditing;
using AwbStudio.TimelineValuePainters;
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
    public partial class ServoValueEditorControl : UserControl, ITimelineEditorControl
    {
        private readonly Brush _gridLineBrush = new SolidColorBrush(Color.FromRgb(60, 60, 100));
        private const double _paintMarginTopBottom = 30;

        private TimelineData? _timelineData;
        private TimelineViewContext? _viewContext;
        private IServo? _servo;
        private TimelineCaption? _caption;
        private bool _isInitialized;
        private ServoValuePainter? _servoValuePainter;

        private void ViewContext_Changed(object? sender, EventArgs e)
        {
            MyInvoker.Invoke(new Action(() => this.PaintServoValues()));
        }

        public ServoValueEditorControl()
        {
            InitializeComponent();
            Loaded += ServoValueViewerControl_Loaded;
        }
        private void ServoValueViewerControl_Loaded(object sender, RoutedEventArgs e)
        {
            DrawOpticalGrid();
            SizeChanged += ServoValueViewerControl_SizeChanged;
            Unloaded += ServoValueViewerControl_Unloaded;
        }

        private void ServoValueViewerControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Unloaded -= ServoValueViewerControl_Unloaded;
            if (_viewContext != null) _viewContext.Changed -= ViewContext_Changed;
            if (_timelineData != null) _timelineData.OnContentChanged -= TimeLineContent_Changed;
        }

        public void Init(IServo servo, TimelineViewContext viewContext, TimelineCaptions timelineCaptions, PlayPosSynchronizer playPosSynchronizer, IActuatorsService actuatorsService)
        {
            _viewContext = viewContext;
            _viewContext.Changed += ViewContext_Changed;
            _servo = servo;
            _servoValuePainter = new ServoValuePainter(servo, AllValuesGrid, _viewContext, timelineCaptions);
            _caption = timelineCaptions?.GetAktuatorCaption(servo.Id) ?? new TimelineCaption { ForegroundColor = new SolidColorBrush(Colors.White) };
            HeaderControl.TimelineCaption = _caption;

            _isInitialized = true;
        }

        public void TimelineDataLoaded(TimelineData timelineData)
        {
            if (_timelineData != null) _timelineData!.OnContentChanged -= TimeLineContent_Changed;
            _timelineData = timelineData;
            _timelineData.OnContentChanged += TimeLineContent_Changed;
            PaintServoValues();
        }

        private void ServoValueViewerControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawOpticalGrid();
        }

        private void TimeLineContent_Changed(object? sender, EventArgs e) =>
            MyInvoker.Invoke(new Action(() => this.PaintServoValues()));

        private void PaintServoValues()
        {
            if (!_isInitialized) throw new InvalidOperationException(Name + " not initialized");

            if (_timelineData == null) return;

            AllValuesGrid.Children.Clear();

            double height = this.ActualHeight;
            double width = this.ActualWidth;

            if (height < 100 || width < 100) return;

            var servoIds = _timelineData?.ServoPoints?.OfType<ServoPoint>().Select(p => p.ServoId).Where(id => id != null).Distinct().ToArray() ?? Array.Empty<string>();

            double diagramHeight = height - _paintMarginTopBottom * 2;

            // ToDo: cache and only update on changes; or: use model binding and auto update

            // Add polylines with points
            var pointsForThisServo = _timelineData?.ServoPoints.OfType<ServoPoint>().Where(p => p.ServoId == _servo.Id).OrderBy(p => p.TimeMs).ToList() ?? new List<ServoPoint>();

            // add dots
            const int dotRadius = 3;
            const int dotWidth = dotRadius * 2;
            foreach (var point in pointsForThisServo)
            {
                if (point.TimeMs >= 0 && point.TimeMs <= _viewContext!.DurationMs) // is inside view
                {
                    this.AllValuesGrid.Children.Add(new Ellipse
                    {
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        Fill = _caption.ForegroundColor,
                        Stroke = _caption.ForegroundColor,
                        Height = dotWidth,
                        Width = dotWidth,
                        Margin = new Thickness { Left = _viewContext.GetXPos(timeMs: (int)point.TimeMs, timelineData: _timelineData) - dotRadius, Top = height - _paintMarginTopBottom - point.ValuePercent / 100.0 * diagramHeight - dotRadius },
                        ToolTip = point.Title
                    });
                }
            }

            var points = new PointCollection(pointsForThisServo.Select(p =>
            new Point
            {
                X = _viewContext!.GetXPos((int)(p.TimeMs), timelineData: _timelineData),
                Y = height - _paintMarginTopBottom - p.ValuePercent / 100.0 * diagramHeight
            }));
            var line = new Polyline { Tag = ServoTag(_servo.Id), Stroke = _caption.ForegroundColor, StrokeThickness = 1, Points = points };
            this.AllValuesGrid.Children.Add(line);
        }

        private void DrawOpticalGrid()
        {
            OpticalGrid.Children.Clear();

            double height = this.ActualHeight;
            double width = this.ActualWidth;

            if (height < 100 || width < 100) return;

            double diagramHeight = height - _paintMarginTopBottom * 2;

            foreach (var valuePercent in new[] { 0, 25, 50, 75, 100 })
            {
                var y = height - _paintMarginTopBottom - valuePercent / 100.0 * diagramHeight;
                OpticalGrid.Children.Add(new Line { X1 = 0, X2 = width, Y1 = y, Y2 = y, Stroke = _gridLineBrush });
            }
        }

        private static string ServoTag(string servoId) => $"Servo {servoId}";
    }
}

// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Actuators;
using Awb.Core.Timelines;
using AwbStudio.TimelineControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AwbStudio.TimelineValuePainters
{
    class ServoValuePainter : ITimelineValuePainter
    {
        private const double _paintMarginTopBottom = 0;

        private readonly IServo _servo;
        private readonly TimelineViewContext _viewContext;
        private readonly TimelineCaptions _timelineCaptions;
        private TimelineData _timelineData;


        public ServoValuePainter(IServo servo, Grid paintControl, TimelineViewContext viewContext, TimelineCaptions timelineCaptions)
        {
            _servo = servo;
            _viewContext = viewContext;
            _timelineCaptions = timelineCaptions;
            this.PaintControl = paintControl;
        }
        public Grid PaintControl { get; }

        public void TimelineDataLoaded(TimelineData timelineDate)
        {
            _timelineData = timelineDate;
        }

        public void PaintValues()
        {
            //if (!_isInitialized) throw new InvalidOperationException(Name + " not initialized");

            if (_timelineData == null) return;

            double height = PaintControl.ActualHeight;
            double width = PaintControl.ActualWidth;

            if (height < 20 || width < 100) return;

            var servoIds = _timelineData?.ServoPoints?.OfType<ServoPoint>().Select(p => p.ServoId).Where(id => id != null).Distinct().ToArray() ?? Array.Empty<string>();

            double diagramHeight = height - _paintMarginTopBottom * 2;

            // Update the content points and lines
            // ToDo: cache and only update on changes; or: use model binding and auto update

            var caption = _timelineCaptions?.GetAktuatorCaption(_servo.Id) ?? new TimelineCaption { ForegroundColor = new SolidColorBrush(Colors.White) };

            // Add polylines with points
            var pointsForThisServo = _timelineData?.ServoPoints.OfType<ServoPoint>().Where(p => p.ServoId == _servo.Id).OrderBy(p => p.TimeMs).ToList() ?? new List<ServoPoint>();

            // add dots
            const int dotRadius = 3;
            const int dotWidth = dotRadius * 2;
            foreach (var point in pointsForThisServo)
            {
                if (point.TimeMs >= 0 && point.TimeMs <= _viewContext!.DurationMs) // is inside view
                {
                    this.PaintControl.Children.Add(new Ellipse
                    {
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        Fill = caption.ForegroundColor,
                        Stroke = caption.ForegroundColor,
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
            var line = new Polyline { Tag = ServoTag(_servo.Id), Stroke = caption.ForegroundColor, StrokeThickness = 1, Points = points };
            this.PaintControl.Children.Add(line);
        }

        private static string ServoTag(string servoId) => $"Servo {servoId}";
    }
}

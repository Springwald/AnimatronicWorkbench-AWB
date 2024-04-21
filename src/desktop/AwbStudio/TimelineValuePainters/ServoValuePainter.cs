// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Actuators;
using Awb.Core.Timelines;
using AwbStudio.TimelineEditing;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AwbStudio.TimelineValuePainters
{
    class ServoValuePainter : AbstractValuePainter
    {
        private const double _paintMarginTopBottom = 0;
        private readonly double _dotRadius;
        private readonly IServo _servo;
        private readonly TimelineCaptions _timelineCaptions;

        public ServoValuePainter(IServo servo, Grid paintControl, TimelineViewContext viewContext, TimelineCaptions timelineCaptions, double dotRadius = 3)
            : base(paintControl, viewContext, timelineCaptions)
        {
            _dotRadius = dotRadius;
            _servo = servo;
            _timelineCaptions = timelineCaptions;
        }

        protected override void TimelineDataLoadedInternal()
        {
        }

        protected override void PaintValues()
        {
            if (_timelineData == null) return;

            base.CleanUpValueControls(); // todo: only remove changed or not confirmed controls

            double height = PaintControl.ActualHeight;
            double width = PaintControl.ActualWidth;

            if (height < _paintMarginTopBottom * 2 || width < 100) return;

            double diagramHeight = height - _paintMarginTopBottom * 2;

            // Update the content points and lines
            // ToDo: cache and only update on changes; or: use model binding and auto update

            var caption = _timelineCaptions?.GetAktuatorCaption(_servo.Id) ?? new TimelineCaption (foregroundColor: new SolidColorBrush(Colors.White) );

            // Add polylines with points
            var pointsForThisServo = _timelineData?.ServoPoints.OfType<ServoPoint>().Where(p => p.ServoId == _servo.Id).OrderBy(p => p.TimeMs).ToList() ?? new List<ServoPoint>();

            // add dots
            double dotWidth = _dotRadius * 2;
            foreach (var point in pointsForThisServo)
            {
                if (point.TimeMs >= 0 && point.TimeMs <= _viewContext!.DurationMs) // is inside view
                {
                    var ellipse = new Ellipse
                    {
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        Fill = caption.ForegroundColor,
                        Stroke = caption.ForegroundColor,
                        Height = dotWidth,
                        Width = dotWidth,
                        Margin = new Thickness { Left = _viewContext.GetXPos(timeMs: (int)point.TimeMs, timelineData: _timelineData) - _dotRadius, Top = height - _paintMarginTopBottom - point.ValuePercent / 100.0 * diagramHeight - _dotRadius },
                        ToolTip = point.Title
                    };
                    this.PaintControl.Children.Add(ellipse);
                    _valueControls.Add(ellipse);
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
            this._valueControls.Add(line);
        }

        private static string ServoTag(string servoId) => $"Servo {servoId}";

        public new void Dispose()
        {
            base.Dispose();
        }

        protected override bool IsChangedEventSuitableForThisPainter(TimelineDataChangedEventArgs changedEventArgs)
        {
            if (changedEventArgs.ChangeType != TimelineDataChangedEventArgs.ChangeTypes.ServoPointChanged) return false;
            return changedEventArgs.ChangedObjectId == _servo.Id;
        }
    }
}

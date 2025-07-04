﻿// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Actuators;
using Awb.Core.Services;
using Awb.Core.Sounds;
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
        private readonly double _dotRadius;
        private readonly IServo _servo;
        private readonly TimelineCaptions _timelineCaptions;
        private readonly ITimelineDataService _timelineDataService;
        private readonly Sound[] _projectSounds;
        private readonly IAwbLogger _awbLogger;
        private readonly OpticalGridCalculator _opticalGridCalculator;

        public ServoValuePainter(IServo servo, Grid paintControl, TimelineViewContext viewContext, TimelineCaptions timelineCaptions, ITimelineDataService timelineDataService, Sound[] projectSounds, IAwbLogger awbLogger, double dotRadius = 3)
            : base(paintControl, viewContext, timelineCaptions)
        {
            _dotRadius = dotRadius;
            _servo = servo;
            _timelineCaptions = timelineCaptions;
            _timelineDataService = timelineDataService;
            _projectSounds = projectSounds;
            _awbLogger = awbLogger;

            _opticalGridCalculator = new OpticalGridCalculator(paintControl);
        }

        protected override void TimelineDataLoadedInternal()
        {
        }

        protected override void PaintValues(IEnumerable<TimelinePoint>? timelinePoints)
        {
            base.CleanUpValueControls(); // todo: only remove changed or not confirmed controls

            if (timelinePoints == null || timelinePoints.Any() == false) return;

            if (PaintControl.ActualHeight < 50 || PaintControl.ActualWidth < 100) return;

            // Update the content points and lines
            // ToDo: cache and only update on changes; or: use model binding and auto update
            var caption = _timelineCaptions?.GetAktuatorCaption(_servo.Id) ?? new TimelineCaption(Brushes.LightSalmon, _servo.Id, label: _servo.Title);

            // Add polylines with points for nested timelines and sounds
            var merger = new EverythingMerger(_timelineDataService, _projectSounds, [_servo], _awbLogger);
            var pointsForThisServo = merger.Merge(timelinePoints).OfType<ServoPoint>().Where(p => p.ServoId == _servo.Id).OrderBy(p => p.TimeMs).ToList() ?? new List<ServoPoint>();

            // add dots
            double dotWidth = _dotRadius * 2;
            foreach (var point in pointsForThisServo)
            {
                if (point.TimeMs >= 0 && point.TimeMs <= _viewContext!.DurationMs) // is inside view
                {
                    Shape shape;

                    if (!double.NaN.Equals(point.ValuePercent))
                    {

                        if (point.IsNestedTimelinePoint)
                        {
                            shape = new Rectangle
                            {
                                HorizontalAlignment = HorizontalAlignment.Left,
                                VerticalAlignment = VerticalAlignment.Top,
                                Fill = Brushes.Transparent,
                                Margin = new Thickness
                                {
                                    Left = _viewContext.GetXPos(timeMs: (int)point.TimeMs, timelineData: _timelineData) - _dotRadius,
                                    Top = _opticalGridCalculator.GetYForPercentValue(point.ValuePercent) - _dotRadius
                                }
                            };

                        }
                        else
                        {
                            var left = _viewContext.GetXPos(timeMs: (int)point.TimeMs, timelineData: _timelineData) - _dotRadius;
                            var top = _opticalGridCalculator.GetYForPercentValue(point.ValuePercent) - _dotRadius;
                            shape = new Ellipse
                            {
                                HorizontalAlignment = HorizontalAlignment.Left,
                                VerticalAlignment = VerticalAlignment.Top,
                                Fill = caption.ForegroundColor,
                                Margin = new Thickness { Left = left, Top = top }
                            };
                        }

                        shape.Height = dotWidth;
                        shape.Width = dotWidth;
                        shape.ToolTip = point.Title;
                        shape.Stroke = caption.ForegroundColor;
                        this.PaintControl.Children.Add(shape);
                        _valueControls.Add(shape);
                    }
                }
            }

            var points = new PointCollection(pointsForThisServo.Select(p =>
            new Point
            {
                X = _viewContext!.GetXPos((int)(p.TimeMs), timelineData: _timelineData),
                Y = _opticalGridCalculator.GetYForPercentValue(p.ValuePercent),
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
            if (changedEventArgs.ChangeType == TimelineDataChangedEventArgs.ChangeTypes.NestedTimelinePointChanged) return true;
            if (changedEventArgs.ChangeType != TimelineDataChangedEventArgs.ChangeTypes.ServoPointChanged) return false;
            return changedEventArgs.ChangedObjectId == _servo.Id;
        }
    }
}

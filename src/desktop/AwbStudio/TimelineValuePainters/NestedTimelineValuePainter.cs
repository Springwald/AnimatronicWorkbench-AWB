﻿// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Timelines;
using AwbStudio.FileManagement;
using AwbStudio.PropertyControls;
using AwbStudio.TimelineEditing;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AwbStudio.TimelineValuePainters
{
    internal class NestedTimelineValuePainter : AbstractValuePainter
    {
        private const int _paintMarginTopBottom = 0;
        private readonly TimelineCaptions _timelineCaptions;
        private readonly ITimelineMetaDataService _timelineMetaDataService;

        public NestedTimelineValuePainter(Grid paintControl, TimelineViewContext viewContext, TimelineCaptions timelineCaptions, ITimelineMetaDataService timelineMetaDataService) :
            base(paintControl, viewContext, timelineCaptions)
        {
            _timelineCaptions = timelineCaptions;
            _timelineMetaDataService = timelineMetaDataService;
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

            //var soundPlayerIds = _timelineData?.SoundPoints?.OfType<SoundPoint>().Select(p => p.SoundPlayerId).Where(id => id != null).Distinct().ToArray() ?? Array.Empty<string>();

            double diagramHeight = height - _paintMarginTopBottom * 2;

            // Update the content points and lines
            // ToDo: cache and only update on changes; or: use model binding and auto update

            double y = 0;

            var caption = _timelineCaptions?.GetAktuatorCaption(NestedTimelinesFakeObject.Singleton.Id) ?? new TimelineCaption { ForegroundColor = new SolidColorBrush(Colors.LightSkyBlue) };

            // Add polylines with points
            var points = _timelineData?.NestedTimelinePoints.OfType<NestedTimelinePoint>().OrderBy(p => p.TimeMs).ToList() ?? new List<NestedTimelinePoint>();

            // add dots
            foreach (var point in points)
            {
                y += 20;
                if (y > 60) y = 0;
                if (point.TimeMs >= 0 && point.TimeMs <= _viewContext.DurationMs) // is inside view
                {
                    var timeMs = (int)point.TimeMs;

                    if (_timelineMetaDataService.ExistsTimeline(point.TimelineId) == false)
                    {
                        var label = new NestedTimelinePointLabel()
                        {
                            LabelText = $"TIMELINE + " + point.TimelineId + " NOT FOUND!",
                            Margin = new Thickness
                            {
                                Left = _viewContext.GetXPos(timeMs: timeMs, timelineData: _timelineData),
                                Top = y,
                                Bottom = 0
                            },
                        };
                        _valueControls.Add(label);
                        PaintControl.Children.Add(label);
                    }
                    else
                    {
                        var timeline = _timelineMetaDataService.GetMetaData(point.TimelineId);
                        var label = new NestedTimelinePointLabel()
                        {
                            LabelText = $"Timeline: {timeline.Title ?? point.Title}",
                            Margin = new Thickness
                            {
                                Left = _viewContext.GetXPos(timeMs: timeMs, timelineData: _timelineData),
                                Top = y,
                                Bottom = 0
                            },
                        };
                        _valueControls.Add(label);
                        PaintControl.Children.Add(label);
                        var durationMs = _timelineMetaDataService.GetDurationMs(point.TimelineId);
                        if (durationMs < 1000) durationMs = 1000;
                        label.SetWidthByDuration(_viewContext.PixelPerMs * durationMs);
                    }

                }
            }
        }

        public new void Dispose()
        {
            base.Dispose();
        }

        protected override bool IsChangedEventSuitableForThisPainter(TimelineDataChangedEventArgs changedEventArgs)
        {
            if (changedEventArgs.ChangeType != TimelineDataChangedEventArgs.ChangeTypes.NestedTimelinePointChanged) return false;
            return NestedTimelinesFakeObject.Singleton.Id == changedEventArgs.ChangedObjectId;
        }
    }
}

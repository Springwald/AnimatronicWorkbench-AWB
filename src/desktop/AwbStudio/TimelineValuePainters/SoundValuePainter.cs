// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Actuators;
using Awb.Core.Timelines;
using AwbStudio.TimelineControls.PropertyControls;
using AwbStudio.TimelineEditing;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AwbStudio.TimelineValuePainters
{
    class SoundValuePainter : AbstractValuePainter
    {
        private const int _paintMarginTopBottom = 0;
        private readonly ISoundPlayer _soundPlayer;
        private readonly TimelineCaptions _timelineCaptions;

        public SoundValuePainter(ISoundPlayer soundPlayer, Grid paintControl, TimelineViewContext viewContext, TimelineCaptions timelineCaptions) :
            base(paintControl, viewContext, timelineCaptions)
        {
            _soundPlayer = soundPlayer;
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

            //var soundPlayerIds = _timelineData?.SoundPoints?.OfType<SoundPoint>().Select(p => p.SoundPlayerId).Where(id => id != null).Distinct().ToArray() ?? Array.Empty<string>();

            double diagramHeight = height - _paintMarginTopBottom * 2;

            // Update the content points and lines
            // ToDo: cache and only update on changes; or: use model binding and auto update

            double y = 0;

            var caption = _timelineCaptions?.GetAktuatorCaption(_soundPlayer.Id) ?? new TimelineCaption { ForegroundColor = new SolidColorBrush(Colors.LightYellow) };

            // Add polylines with points
            var pointsForThisSoundplayer = _timelineData?.SoundPoints.OfType<SoundPoint>().Where(p => p.SoundPlayerId == _soundPlayer.Id).OrderBy(p => p.TimeMs).ToList() ?? new List<SoundPoint>();

            // add dots
            foreach (var point in pointsForThisSoundplayer)
            {
                if (point.TimeMs >= 0 && point.TimeMs <= _viewContext.DurationMs) // is inside view
                {
                    var label = new SoundPlayerPointLabel()
                    {
                        LabelText = "Sound" + point.Title,
                        Margin = new Thickness
                        {
                            Left = _viewContext.GetXPos(timeMs: (int)point.TimeMs, timelineData: _timelineData),
                            Bottom = 0
                        }
                    };
                    PaintControl.Children.Add(label);
                }
            }
        }

        public new void Dispose()
        {
            base.Dispose();
        }
    }
}

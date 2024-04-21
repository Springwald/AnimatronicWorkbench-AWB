// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Actuators;
using Awb.Core.Sounds;
using Awb.Core.Timelines;
using AwbStudio.PropertyControls;
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
        private readonly Sound[] _projectSounds;

        public SoundValuePainter(ISoundPlayer soundPlayer, Grid paintControl, TimelineViewContext viewContext, TimelineCaptions timelineCaptions, Sound[] projectSounds) :
            base(paintControl, viewContext, timelineCaptions)
        {
            _soundPlayer = soundPlayer;
            _timelineCaptions = timelineCaptions;
            _projectSounds = projectSounds;
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

            var caption = _timelineCaptions?.GetAktuatorCaption(_soundPlayer.Id) ?? new TimelineCaption (foregroundColor: new SolidColorBrush(Colors.LightYellow) );

            // Add polylines with points
            var pointsForThisSoundplayer = _timelineData?.SoundPoints.OfType<SoundPoint>().Where(p => p.SoundPlayerId == _soundPlayer.Id).OrderBy(p => p.TimeMs).ToList() ?? new List<SoundPoint>();

            // add dots
            foreach (var point in pointsForThisSoundplayer)
            {
                y += 20;
                if (y > 60) y = 0;
                if (point.TimeMs >= 0 && point.TimeMs <= _viewContext.DurationMs) // is inside view
                {
                    var timeMs = (int)point.TimeMs;

                    var sound = _projectSounds.FirstOrDefault(s => s.Id == point.SoundId);

                    var label = new SoundPlayerPointLabel()
                    {
                        LabelText = $"Sound: {sound?.Title ?? point.Title}",
                        Margin = new Thickness
                        {
                            Left = _viewContext.GetXPos(timeMs: timeMs, timelineData: _timelineData),
                            Top = y,
                            Bottom = 0
                        },
                    };

                    _valueControls.Add(label);  
                    PaintControl.Children.Add(label);

                    // set the with to the duration of the sound if available
                   
                    var durationMs = sound?.DurationMs ?? 1000;
                    label.SetWidthByDuration(_viewContext.PixelPerMs * durationMs);
                }
            }
        }

        public new void Dispose()
        {
            base.Dispose();
        }

        protected override bool IsChangedEventSuitableForThisPainter(TimelineDataChangedEventArgs changedEventArgs)
        {
            if (changedEventArgs.ChangeType != TimelineDataChangedEventArgs.ChangeTypes.SoundPointChanged) return false;
            return this._soundPlayer.Id == changedEventArgs.ChangedObjectId;
        }
    }
}

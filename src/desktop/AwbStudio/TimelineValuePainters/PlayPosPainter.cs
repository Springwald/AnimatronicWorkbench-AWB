// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Player;
using Awb.Core.Timelines;
using AwbStudio.TimelineControls;
using AwbStudio.TimelineEditing;
using AwbStudio.Tools;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace AwbStudio.TimelineValuePainters
{
    class PlayPosPainter : IDisposable
    {
        private readonly Grid _paintControl;
        private readonly TimelineViewContext _viewContext;
        private readonly PlayPosSynchronizer _playPosSynchronizer;
        private Label _labelPlayPosAbsoluteControl;
        private Line _linePlayPosControl;
        private TimelineData? _timelineData;

        public PlayPosPainter(Grid paintControl, TimelineViewContext timelineViewContext, PlayPosSynchronizer playPosSynchronizer)
        {
            _paintControl = paintControl;
            _viewContext = timelineViewContext;
            _playPosSynchronizer = playPosSynchronizer;

            var prototypePaintControl = new TimelinePrototypeControls();
            _labelPlayPosAbsoluteControl = prototypePaintControl.LabelPlayPosAbsoluteControl;
            _linePlayPosControl = prototypePaintControl.LinePlayPosControl;
            _paintControl.Children.Add(_labelPlayPosAbsoluteControl);
            _paintControl.Children.Add(_linePlayPosControl);

            _playPosSynchronizer.OnPlayPosChanged += OnPlayPosChanged;
            _viewContext.Changed += OnViewContextChanged;
            _paintControl.SizeChanged += OnPaintControlSizeChanged;

            PaintPlayPos();
        }

        private void OnPaintControlSizeChanged(object sender, SizeChangedEventArgs e)
        {
            PaintPlayPos();
        }

        private void OnViewContextChanged(object? sender, EventArgs e)
        {
            PaintPlayPos();
        }

        private void OnPlayPosChanged(object? sender, int e)
        {
            //MyInvoker.Invoke(new Action(() =>
            PaintPlayPos();
            //));
        }

        public void TimelineDataLoaded(TimelineData timelineData)
        {
            _timelineData = timelineData;
            PaintPlayPos();
        }

        private void PaintPlayPos()
        {

            // draw the manual midi controller play position as triangle at the bottom
            var x = _viewContext.GetXPos(_playPosSynchronizer.PlayPosMsAutoSnappedOrUnSnapped);
            _labelPlayPosAbsoluteControl.Margin = new Thickness(x - _labelPlayPosAbsoluteControl.FontSize, 0, 0, 0);
            _labelPlayPosAbsoluteControl.Content = $"🔺 {_playPosSynchronizer.PlayPosMsAutoSnappedOrUnSnapped}ms";

            // draw the play position as a vertical line
            var playPosMs = _playPosSynchronizer.PlayPosMsAutoSnappedOrUnSnapped;
            if (_timelineData == null || playPosMs < 0 || playPosMs > _viewContext.DurationMs)
            {
                _linePlayPosControl.Visibility = Visibility.Hidden;
            }
            else
            {
                x = this._viewContext.GetXPos(timeMs: playPosMs);
                _linePlayPosControl.X1 = x;
                _linePlayPosControl.X2 = x;
                _linePlayPosControl.Y1 = 0;
                _linePlayPosControl.Y2 = _paintControl.ActualHeight;
                _linePlayPosControl.Visibility = Visibility.Visible;
            }
        }

        public void Dispose()
        {
            if (_labelPlayPosAbsoluteControl != null)
                _paintControl.Children.Remove(_labelPlayPosAbsoluteControl);
            if (_linePlayPosControl != null)
                _paintControl.Children.Remove(_linePlayPosControl);

            _playPosSynchronizer.OnPlayPosChanged -= OnPlayPosChanged;
            _viewContext.Changed -= OnViewContextChanged;
            _paintControl.SizeChanged -= OnPaintControlSizeChanged;

        }
    }
}

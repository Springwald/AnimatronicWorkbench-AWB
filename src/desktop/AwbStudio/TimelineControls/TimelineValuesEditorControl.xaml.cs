// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Player;
using Awb.Core.Services;
using Awb.Core.Timelines;
using AwbStudio.TimelineValuePainters;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AwbStudio.TimelineControls
{
    public partial class TimelineValuesEditorControl : UserControl, ITimelineEditorControl
    {
        private bool _wasPlaying = false;
        private readonly Brush _gridLineBrush = new SolidColorBrush(Color.FromRgb(60, 60, 100));
        private TimelineData? _timelineData;

        private IActuatorsService? _actuatorsService;
        private TimelineCaptions _timelineCaptions;
        private TimelineViewContext _viewContext;
        private PlayPosSynchronizer _playPosSynchronizer;
        private bool _isInitialized;
        private TimelinePlayer? _timelinePlayer;


        private List<ITimelineEditorControl> _timelineEditorControls;
        private List<ITimelineValuePainter> _timelineValuePainters;

        private double _zoomVerticalHeightPerValueEditor = 180; // pixel per value editor

        /// <summary>
        /// The timeline player to control the timeline playback
        /// </summary>
        public TimelinePlayer? Timelineplayer
        {
            get => _timelinePlayer;
            set
            {
                _timelinePlayer = value;
                if (_timelinePlayer != null)
                {

                }
            }
        }

        public TimelineValuesEditorControl()
        {
            InitializeComponent();

            //MyViewModel vm = new MyViewModel();
            SizeChanged += TimelineViewer_SizeChanged;
        }

        public void Init(TimelineViewContext viewContext, TimelineCaptions timelineCaptions, PlayPosSynchronizer playPosSynchronizer, IActuatorsService actuatorsService)
        {
            _viewContext = viewContext;
            _timelineCaptions = timelineCaptions;
            _playPosSynchronizer = playPosSynchronizer;
            _actuatorsService = actuatorsService;

            _viewContext.Changed += OnViewContextChanged;
            _playPosSynchronizer.OnPlayPosChanged += (sender, e) => MyInvoker.Invoke(new Action(() => { PaintPlayPos(); }));


            // set up the actuator value painters and editors
            _timelineEditorControls = new List<ITimelineEditorControl>();
            _timelineValuePainters = new List<ITimelineValuePainter>();

            // add servo painter + editors
            foreach (var servoActuator in actuatorsService.Servos)
            {
                _timelineValuePainters.Add(new ServoValuePainter(
                    servo: servoActuator,
                    paintControl: this.AllValuesGrid,
                    viewContext: _viewContext,
                    timelineCaptions: _timelineCaptions));

                var editorControl = new ServoValueEditorControl();
                editorControl.Init(servo: servoActuator, viewContext, timelineCaptions, playPosSynchronizer, actuatorsService);
                AllValueEditorControlsScrollViewer.Children.Add(editorControl);
                _timelineEditorControls.Add(editorControl);
            }

            // add sound painter + editors
            foreach (var soundPlayerActuator in actuatorsService.SoundPlayers)
            {
                _timelineValuePainters.Add(new SoundValuePainter(soundPlayer: soundPlayerActuator, paintControl: this.AllValuesGrid));

                var editorControl = new SoundValueEditorControl();
                editorControl.Init(
                    soundPlayer: soundPlayerActuator, 
                    viewContext, 
                    timelineCaptions, 
                    playPosSynchronizer, 
                    actuatorsService);
                AllValueEditorControlsScrollViewer.Children.Add(editorControl);
                _timelineEditorControls.Add(editorControl);
            }

            // todo: add nested timelines painter + editors

            ZoomChanged();

            _isInitialized = true;
        }

        public void TimelineDataLoaded(TimelineData timelineData)
        {
            if (!_isInitialized) throw new InvalidOperationException(Name + " not initialized");

            _timelineData = timelineData;

            foreach (var subTimelineEditorControl in _timelineEditorControls)
                subTimelineEditorControl.TimelineDataLoaded(timelineData);

            foreach (var valuePainter in _timelineValuePainters)
                valuePainter.TimelineDataLoaded(timelineData);

            PaintGrid();
            PaintPlayPos();
            PaintValues();
        }

        private void PaintValues()
        {
            AllValuesGrid.Children.Clear();
            foreach (var valuePainter in _timelineValuePainters)
            {
                valuePainter.PaintValues();
            }
        }

        private void ZoomChanged()
        {
            foreach (UserControl editorControl in _timelineEditorControls)
                editorControl.Height = _zoomVerticalHeightPerValueEditor;
        }

        public void PaintGrid()
        {
            if (!_isInitialized) throw new InvalidOperationException(Name + " not initialized");

            const double _paintMarginTopBottom = 30;
            double height = OpticalGrid.ActualHeight;
            double width = OpticalGrid.ActualWidth;

            if (height < 10 || width < 100) return;

            // update the data optical grid lines
            OpticalGrid.Children.Clear();
            var duration = _viewContext.DurationMs;
            const int STEP = 1000;
            for (int ms = 0; ms < duration; ms += STEP)
            {
                var x = _viewContext.GetXPos(ms);
                if (x > 0 && x < width)
                {
                    OpticalGrid.Children.Add(new Line { X1 = x, X2 = x, Y1 = _paintMarginTopBottom, Y2 = height - _paintMarginTopBottom, Stroke = _gridLineBrush });
                    OpticalGrid.Children.Add(new Label { Content = ((ms) / STEP).ToString(), BorderThickness = new Thickness(left: x, top: height - 30, right: 0, bottom: 0) });
                }
            }
        }

        private void PaintPlayPos()
        {
            if (!_isInitialized) throw new InvalidOperationException(Name + " not initialized");

            // draw the manual midi controller play position as triangle at the bottom
            var x = _viewContext.GetXPos(_playPosSynchronizer.PlayPosMs);
            LabelManualPlayPosAbsolute.Margin = new Thickness(x - LabelManualPlayPosAbsolute.FontSize, 0, 0, 0);
            LabelManualPlayPosAbsolute.Content = $"🔺 {_playPosSynchronizer.PlayPosMs}ms";

            // draw the play position as a vertical line
            var playPosMs = _playPosSynchronizer.PlayPosMs;
            if (_timelineData == null || playPosMs < 0 || playPosMs > _viewContext.DurationMs)
            {
                PlayPosLine.Visibility = Visibility.Hidden;
            }
            else
            {
                x = this._viewContext.GetXPos(timeMs: playPosMs);
                PlayPosLine.X1 = x;
                PlayPosLine.X2 = x;
                PlayPosLine.Y1 = 0;
                PlayPosLine.Y2 = this.ActualHeight;
                PlayPosLine.Visibility = Visibility.Visible;
            }
        }



        private void OnViewContextChanged(object? sender, EventArgs e)
        {
            if (_timelineData == null) return;



            var newWidth = this._viewContext.PixelPerMs * this._viewContext.DurationMs;

            // update the scrollbar
            MyInvoker.Invoke(() =>
            {
                this.Width = newWidth;
                PaintValues();
            });
        }

        private void TimelineViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!_isInitialized) return;
            PaintGrid();
            PaintValues();
        }


    }
}
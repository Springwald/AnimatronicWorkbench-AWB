// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Player;
using Awb.Core.Services;
using Awb.Core.Timelines;
using AwbStudio.TimelineEditing;
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
     
        private TimelineData? _timelineData;

        private IActuatorsService? _actuatorsService;
        private TimelineCaptions _timelineCaptions;
        private TimelineViewContext? _viewContext;
        private PlayPosSynchronizer? _playPosSynchronizer;
        private bool _isInitialized;
        private TimelinePlayer? _timelinePlayer;

        private List<ITimelineEditorControl>? _timelineEditorControls;
        private List<AbstractValuePainter>? _timelineValuePainters;

        private double _zoomVerticalHeightPerValueEditor = 180; // pixel per value editor
        private PlayPosPainter? _playPosPainter;
        private GridPainter? _gridPainter;

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

            Unloaded += OnUnloaded;

            //MyViewModel vm = new MyViewModel();
            SizeChanged += TimelineViewer_SizeChanged;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _playPosPainter?.Dispose();
            _playPosPainter = null;
            _gridPainter?.Dispose();
            _gridPainter = null;
        }

        public void Init(TimelineViewContext viewContext, TimelineCaptions timelineCaptions, PlayPosSynchronizer playPosSynchronizer, IActuatorsService actuatorsService)
        {
            _viewContext = viewContext;
            _timelineCaptions = timelineCaptions;
            _playPosSynchronizer = playPosSynchronizer;
            _actuatorsService = actuatorsService;

            _viewContext.Changed += OnViewContextChanged;
           // _playPosSynchronizer.OnPlayPosChanged += (sender, e) => MyInvoker.Invoke(new Action(() => { PaintPlayPos(); }));

            // set up the actuator value painters and editors
            _timelineEditorControls = [];
            _timelineValuePainters = [];

            // add servo painter + editors
            foreach (var servoActuator in actuatorsService.Servos)
            {
                var editorControl = new ServoValueEditorControl();
                editorControl.Init(servo: servoActuator, viewContext, timelineCaptions, playPosSynchronizer, actuatorsService);
                AllValueEditorControlsScrollViewer.Children.Add(editorControl);
                _timelineEditorControls.Add(editorControl);
            }

            // add sound painter + editors
            foreach (var soundPlayerActuator in actuatorsService.SoundPlayers)
            {
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

            _playPosPainter = new PlayPosPainter(PlayPosGrid, _viewContext, _playPosSynchronizer);
            _gridPainter = new GridPainter(OpticalGrid, _viewContext);

            ZoomChanged();

            _isInitialized = true;
        }

        public void TimelineDataLoaded(TimelineData timelineData)
        {
            if (!_isInitialized) throw new InvalidOperationException(Name + " not initialized");

            _timelineData = timelineData;

            foreach (var subTimelineEditorControl in _timelineEditorControls!)
                subTimelineEditorControl.TimelineDataLoaded(timelineData);

            foreach (var valuePainter in _timelineValuePainters!)
                valuePainter.TimelineDataLoaded(timelineData);

            _playPosPainter!.TimelineDataLoaded(timelineData);
        }

        private void ZoomChanged()
        {
            foreach (UserControl editorControl in _timelineEditorControls)
                editorControl.Height = _zoomVerticalHeightPerValueEditor;
        }

       

        private void OnViewContextChanged(object? sender, EventArgs e)
        {
            if (_timelineData == null) return;

            var newWidth = this._viewContext.PixelPerMs * this._viewContext.DurationMs;

            // update the scrollbar
            MyInvoker.Invoke(() =>
            {
                this.Width = newWidth;
            });
        }

        private void TimelineViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!_isInitialized) return;
            // PaintGrid();
        }


    }
}
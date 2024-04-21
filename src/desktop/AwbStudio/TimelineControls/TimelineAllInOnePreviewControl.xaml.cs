// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Player;
using Awb.Core.Services;
using Awb.Core.Sounds;
using Awb.Core.Timelines;
using AwbStudio.TimelineEditing;
using AwbStudio.TimelineValuePainters;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AwbStudio.TimelineControls
{
    /// <summary>
    /// Interaction logic for TimelineAllInOnePreviewControl.xaml
    /// </summary>
    public partial class TimelineAllInOnePreviewControl : UserControl, ITimelineEditorControl
    {
        private TimelineData? _timelineData;
        private PlayPosPainter? _playPosPainter;
        private GridTimePainter? _gridPainter;
        private Sound[]? _projectSounds;
        private TimelineCaptions? _timelineCaptions;
        private TimelineViewContext? _viewContext;
        private PlayPosSynchronizer? _playPosSynchronizer;
        private TimelinePlayer? _timelinePlayer;
        private List<ITimelineEditorControl>? _timelineEditorControls;
        private List<AbstractValuePainter>? _timelineValuePainters;

        private bool _isInitialized;

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

        public TimelineAllInOnePreviewControl()
        {
            InitializeComponent();
        }

        public void Init(TimelineViewContext viewContext, TimelineCaptions timelineCaptions, PlayPosSynchronizer playPosSynchronizer, IActuatorsService actuatorsService, Sound[] projectSounds)
        {
            _viewContext = viewContext;
            _timelineCaptions = timelineCaptions;
            _playPosSynchronizer = playPosSynchronizer;
            _playPosPainter = new PlayPosPainter(PlayPosGrid, _viewContext, _playPosSynchronizer);
            _gridPainter = new GridTimePainter(OpticalGrid, _viewContext);
            _projectSounds = projectSounds;
            _viewContext.Changed += OnViewContextChanged;

            // set up the actuator value painters and editors
            _timelineEditorControls = [];
            _timelineValuePainters = [];

            // add sound painter + editors
            foreach (var soundPlayerActuator in actuatorsService.SoundPlayers)
            {
                _timelineValuePainters.Add(new SoundValuePainter(
                    soundPlayer: soundPlayerActuator,
                    paintControl: this.AllValuesGrid,
                    viewContext: _viewContext,
                    timelineCaptions: _timelineCaptions,
                    projectSounds: _projectSounds));
            }

            // todo: add nested timelines painter + editors

            // add servo painter + editors
            foreach (var servoActuator in actuatorsService.Servos)
            {
                _timelineValuePainters.Add(new ServoValuePainter(
                    servo: servoActuator,
                    paintControl: this.AllValuesGrid,
                    viewContext: _viewContext,
                    timelineCaptions: _timelineCaptions));
            }


            ZoomChanged();

            Unloaded += OnUnloaded;

            _isInitialized = true;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _playPosPainter?.Dispose();
            _gridPainter?.Dispose();
        }

        public void TimelineDataLoaded(TimelineData timelineData)
        {
            if (!_isInitialized) throw new InvalidOperationException(Name + " not initialized");

            _timelineData = timelineData;
            _playPosPainter!.TimelineDataLoaded(timelineData);

            foreach (var subTimelineEditorControl in _timelineEditorControls!)
                subTimelineEditorControl.TimelineDataLoaded(timelineData);

            foreach (var valuePainter in _timelineValuePainters!)
                valuePainter.TimelineDataLoaded(timelineData);
        }

        private void ZoomChanged()
        {
            if (_timelineEditorControls == null) return;
            foreach (UserControl editorControl in _timelineEditorControls)
                editorControl.Height = _zoomVerticalHeightPerValueEditor;
        }

        private void OnViewContextChanged(object? sender, ViewContextChangedEventArgs e)
        {
            if (_timelineData == null) return;
            if (_viewContext == null) return;

            switch (e.ChangeType)
            {
                case ViewContextChangedEventArgs.ChangeTypes.Duration:
                case ViewContextChangedEventArgs.ChangeTypes.PixelPerMs:
                    var newWidth = this._viewContext.PixelPerMs * this._viewContext.DurationMs;
                    //MyInvoker.Invoke(() =>
                    {
                        this.Width = newWidth;
                    }//);
                    break;

                case ViewContextChangedEventArgs.ChangeTypes.BankIndex:
                case ViewContextChangedEventArgs.ChangeTypes.FocusObjectValue:
                case ViewContextChangedEventArgs.ChangeTypes.Scroll:
                    break;

                case ViewContextChangedEventArgs.ChangeTypes.FocusObject:
                    // todo: hightlight the selectect object lines
                    break;

                default:
                    throw new ArgumentOutOfRangeException($"{nameof(e.ChangeType)}:{e.ChangeType}");
            }
        }

        #region mouse events

        double _mouseX = 0;

        private async void AllValuesGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e) =>
            await SetPlayPosByMouse(_mouseX);

        private async void AllValuesGrid_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            _mouseX = e.GetPosition(AllValuesGrid).X;
            if (e.LeftButton == MouseButtonState.Pressed)
                await SetPlayPosByMouse(_mouseX);
        }

        private async Task SetPlayPosByMouse(double mouseX)
        {
            if (_viewContext == null) return;
            if (_playPosSynchronizer == null) return;

            var newPlayPosMs = (int)(((mouseX) / _viewContext.PixelPerMs) + PlayPosSynchronizer.SnapMs / 2);
            _playPosSynchronizer.SetNewPlayPos(newPlayPosMs);

            await Task.CompletedTask;
        }

        #endregion
    }
}
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
using System.Runtime.CompilerServices;
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
        private double _mouseX = 0;
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

        public void Init(TimelineViewContext viewContext, TimelineCaptions timelineCaptions, PlayPosSynchronizer playPosSynchronizer, IActuatorsService actuatorsService, ITimelineDataService timelineDataService, IAwbLogger awbLogger, Sound[] projectSounds)
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
                    timelineCaptions: _timelineCaptions,
                    timelineDataService: timelineDataService,
                    awbLogger: awbLogger
                    ));
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

            this.Visibility = Visibility.Visible;

            if (_timelineData != null) _timelineData.OnContentChanged -= OnTimelineDataChanged;

            _timelineData = timelineData;
            _playPosPainter!.TimelineDataLoaded(timelineData);

            foreach (var subTimelineEditorControl in _timelineEditorControls!)
                subTimelineEditorControl.TimelineDataLoaded(timelineData);

            foreach (var valuePainter in _timelineValuePainters!)
                valuePainter.TimelineDataLoaded(timelineData);

            _timelineData.OnContentChanged += OnTimelineDataChanged;
            OnTimelineDataChanged(null, null);
            ShowSelectionRectangle();
        }

        private void OnTimelineDataChanged(object? sender, TimelineDataChangedEventArgs? e)
        {
            if (_viewContext == null) return;
            if (_timelineData == null) return;
            this._viewContext.DurationMs = Math.Max(20 * 1000, _timelineData.DurationMs + 5000); // 5000ms extra for the timeline to grow beyond the duration of the last keyframe
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
                    this.Width = newWidth;
                    ShowSelectionRectangle();
                    break;

                case ViewContextChangedEventArgs.ChangeTypes.BankIndex:
                case ViewContextChangedEventArgs.ChangeTypes.FocusObjectValue:
                case ViewContextChangedEventArgs.ChangeTypes.Scroll:
                    break;

                case ViewContextChangedEventArgs.ChangeTypes.FocusObject:
                    // todo: hightlight the selectect object lines
                    break;

                case ViewContextChangedEventArgs.ChangeTypes.Selection:
                    ShowSelectionRectangle();
                    break;

                default:
                    throw new ArgumentOutOfRangeException($"{nameof(e.ChangeType)}:{e.ChangeType}");
            }
        }

        /// <summary>
        /// show and position the timeline selection rectangle
        /// </summary>
        private void ShowSelectionRectangle()
        {
            if (_viewContext == null || _viewContext.SelectionStartMs == null || _viewContext.SelectionEndMs == null)
            {
                // no selection
                SelectionRectangle.Visibility = Visibility.Hidden;
                return;
            }

            //if (_viewContext.SelectionEndMs < _viewContext.SelectionStartMs)
            //{
            //    // Dont allow backwards selection
            //    _viewContext.SelectionEndMs = null;
            //    SelectionRectangle.Visibility = Visibility.Hidden;
            //    return;
            //}

            var x1 = _viewContext.GetXPos(_viewContext.SelectionStartMs.Value);
            var x2 = _viewContext.GetXPos(_viewContext.SelectionEndMs.Value);
            if (x1 > x2) (x1, x2) = (x2, x1); // flip start and end if start is after end
            SelectionRectangle.Margin = new Thickness(x1, 0, 0, 0);
            SelectionRectangle.Width = x2 - x1;
            SelectionRectangle.Height = AllValuesGrid.ActualHeight;
            SelectionRectangle.Visibility = Visibility.Visible;
        }

        #region mouse events


        private async void  Grid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _mouseX = e.GetPosition(AllValuesGrid).X;
            await SetPlayPosByMouse(_mouseX);
            await SetSelectionStartByMouse(_mouseX);
        }

        private void Grid_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_viewContext == null) return;
            if (_viewContext.SelectionEndMs == null)  _viewContext.SelectionStartMs = null;
        }

        private async void Grid_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _mouseX = e.GetPosition(AllValuesGrid).X;
                await SetPlayPosByMouse(_mouseX);
                await SetSelectionEndByMouse(_mouseX);
            }
        }

  
        private async Task SetSelectionEndByMouse(double mouseX)
        {
            if (_viewContext == null) return;
            if (_playPosSynchronizer == null) return;

            /// set the play position by the mouse position
            var playPosMs = PlayPosSynchronizer.Snap((int)(((mouseX) / _viewContext.PixelPerMs) + PlayPosSynchronizer.SnapMs / 2));

            // set the selection start by the mouse position
            _viewContext.SelectionEndMs = playPosMs;

            ShowSelectionRectangle();

            await Task.CompletedTask;
        }

        private async Task SetSelectionStartByMouse(double mouseX)
        {
            if (_viewContext == null) return;
            if (_playPosSynchronizer == null) return;

            /// set the play position by the mouse position
            var playPosMs = PlayPosSynchronizer.Snap((int)(((mouseX) / _viewContext.PixelPerMs) + PlayPosSynchronizer.SnapMs / 2));

            // set the selection start by the mouse position
            _viewContext.SelectionStartMs = playPosMs;
            _viewContext.SelectionEndMs = null;

            ShowSelectionRectangle();

            await Task.CompletedTask;
        }

        private async Task SetPlayPosByMouse(double mouseX)
        {
            if (_viewContext == null) return;
            if (_playPosSynchronizer == null) return;

            /// set the play position by the mouse position
            var newPlayPosMs = (int)(((mouseX) / _viewContext.PixelPerMs) + PlayPosSynchronizer.SnapMs / 2);
            _playPosSynchronizer.SetNewPlayPos(newPlayPosMs);

            await Task.CompletedTask;
        }

        private double GetXPos(int playPosMs, bool snaped)
        {
            return playPosMs * _viewContext.PixelPerMs;
        }

        #endregion


    }
}
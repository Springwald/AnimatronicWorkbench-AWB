// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Actuators;
using Awb.Core.ActuatorsAndObjects;
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
using System.Windows.Media;

namespace AwbStudio.TimelineControls
{
    public partial class TimelineValuesEditorControl : UserControl, ITimelineEditorControl
    {
        private const int EditorControlsBorderThickness = 10;

        private bool _isInitialized;

        private TimelineData? _timelineData;
        private TimelineViewContext? _viewContext;
        private PlayPosSynchronizer? _playPosSynchronizer;
        private PlayPosPainter? _playPosPainter;
        private GridTimePainter? _gridPainter;

        private List<ITimelineEditorControl>? _timelineEditorControls;
        private List<AbstractValuePainter>? _timelineValuePainters;

        private double _zoomVerticalHeightPerValueEditorBackingField = 180; // pixel per value editor


        public double ZoomVerticalHeightPerValueEditor
        {
            get => _zoomVerticalHeightPerValueEditorBackingField;
            set
            {          // pixel per value editor
                _zoomVerticalHeightPerValueEditorBackingField = value;
                UpdateZoom();
            }
        }


        public TimelineValuesEditorControl()
        {
            InitializeComponent();
            Loaded += TimelineValuesEditorControl_Loaded;
        }

        private void TimelineValuesEditorControl_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= TimelineValuesEditorControl_Loaded;
            Unloaded += OnUnloaded;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _playPosPainter?.Dispose();
            _playPosPainter = null;
            _gridPainter?.Dispose();
            _gridPainter = null;
        }

        public void Init(TimelineViewContext viewContext, TimelineCaptions timelineCaptions, PlayPosSynchronizer playPosSynchronizer, IActuatorsService actuatorsService, ITimelineMetaDataService timelineMetaDataService, ITimelineDataService timelineDataService, IAwbLogger awbLogger, Sound[] projectSounds)
        {
            _viewContext = viewContext;
            _playPosSynchronizer = playPosSynchronizer;
            _viewContext.Changed += OnViewContextChanged;

            // set up the actuator value painters and editors
            _timelineEditorControls = [];
            _timelineValuePainters = [];

            // add nested timelines painter + editors
            var nestedTimelineEditorControl = new NestedTimelinesViewerControl();
            nestedTimelineEditorControl.Init(viewContext, timelineCaptions, playPosSynchronizer, actuatorsService, timelineMetaDataService);
            AllValuesEditorControlsStackPanel.Children.Add(nestedTimelineEditorControl);
            _timelineEditorControls.Add(nestedTimelineEditorControl);

            // add servo painter + editors
            foreach (var servoActuator in actuatorsService.Servos)
            {
                var editorControl = new ServoTimelineEditorControl();
                editorControl.Init(servo: servoActuator, viewContext, timelineCaptions, timelineDataService, awbLogger);
                AllValuesEditorControlsStackPanel.Children.Add(editorControl);
                _timelineEditorControls.Add(editorControl);
            }

            // add sound painter + editors
            foreach (var soundPlayerActuator in actuatorsService.SoundPlayers)
            {
                var editorControl = new SoundTimelineEditorControl();
                editorControl.Init(
                    soundPlayer: soundPlayerActuator,
                    viewContext,
                    timelineCaptions,
                    projectSounds: projectSounds);
                AllValuesEditorControlsStackPanel.Children.Add(editorControl);
                _timelineEditorControls.Add(editorControl);
            }

            _playPosPainter = new PlayPosPainter(PlayPosGrid, _viewContext, _playPosSynchronizer);
            _gridPainter = new GridTimePainter(OpticalTimeGrid, _viewContext);

            UpdateZoom();

            _isInitialized = true;
        }

        public void TimelineDataLoaded(TimelineData? timelineData)
        {
            if (!_isInitialized) throw new InvalidOperationException(Name + " not initialized");

            if (timelineData == null) this.Visibility = Visibility.Hidden;
            else
            {
                this.Visibility = Visibility.Visible;

                _timelineData = timelineData;

                foreach (var subTimelineEditorControl in _timelineEditorControls!)
                    subTimelineEditorControl.TimelineDataLoaded(timelineData);

                foreach (var valuePainter in _timelineValuePainters!)
                    valuePainter.TimelineDataLoaded(timelineData);

                _playPosPainter!.TimelineDataLoaded(timelineData);
            }
        }

        private void UpdateZoom()
        {
            if (_timelineEditorControls != null)
                foreach (UserControl editorControl in _timelineEditorControls)
                {
                    if (editorControl is NestedTimelinesViewerControl || editorControl is SoundTimelineEditorControl)
                    {
                        editorControl.Height = Math.Max(120, _zoomVerticalHeightPerValueEditorBackingField / 4);
                    }
                    else
                    {
                        editorControl.Height = _zoomVerticalHeightPerValueEditorBackingField;
                    }
                }

        }

        public double? GetScrollPosForEditorControl(IAwbObject? awbObject)
        {
            if (awbObject is IServo servo)
                foreach (var child in AllValuesEditorControlsStackPanel.Children)
                {
                    if (child is ServoTimelineEditorControl servoControl)
                    {
                        if (servoControl.Servo?.Id == servo.Id)
                        {
                            var transform = servoControl.TransformToVisual(AllValuesEditorControlsStackPanel as FrameworkElement);
                            Point yPosOfControl = transform.Transform(new Point(0, 0));
                            return yPosOfControl.Y;
                        }
                    }
                }
            return null;
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

                case ViewContextChangedEventArgs.ChangeTypes.Scroll:
                case ViewContextChangedEventArgs.ChangeTypes.BankIndex:
                case ViewContextChangedEventArgs.ChangeTypes.FocusObjectValue:
                    break;

                case ViewContextChangedEventArgs.ChangeTypes.FocusObject:
                    break;

                default:
                    throw new ArgumentOutOfRangeException($"{nameof(e.ChangeType)}:{e.ChangeType}");
            }
        }

    }
}
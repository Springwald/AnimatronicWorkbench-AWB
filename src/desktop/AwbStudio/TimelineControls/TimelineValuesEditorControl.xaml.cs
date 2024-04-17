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
using Awb.Core.Timelines;
using AwbStudio.TimelineControls.PropertyControls;
using AwbStudio.TimelineEditing;
using AwbStudio.TimelineValuePainters;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AwbStudio.TimelineControls
{
    public partial class TimelineValuesEditorControl : UserControl, ITimelineEditorControl
    {
        private TimelineData? _timelineData;

        private TimelineViewContext? _viewContext;
        private PlayPosSynchronizer? _playPosSynchronizer;
        private bool _isInitialized;
        private TimelinePlayer? _timelinePlayer;

        private List<ITimelineEditorControl>? _timelineEditorControls;
        private List<AbstractValuePainter>? _timelineValuePainters;


        private PlayPosPainter? _playPosPainter;
        private GridTimePainter? _gridPainter;
        private SolidColorBrush _highlightBackground;

        public double ZoomVerticalHeightPerValueEditor { get; private set; } = 180; // pixel per value editor

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
            Loaded += TimelineValuesEditorControl_Loaded;
        }

        private void TimelineValuesEditorControl_Loaded(object sender, RoutedEventArgs e)
        {
            _highlightBackground = new SolidColorBrush(Color.FromRgb(50,50,60));
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

        public void Init(TimelineViewContext viewContext, TimelineCaptions timelineCaptions, PlayPosSynchronizer playPosSynchronizer, IActuatorsService actuatorsService)
        {
            _viewContext = viewContext;
            _playPosSynchronizer = playPosSynchronizer;
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
                AllValuesEditorControlsStackPanel.Children.Add(editorControl);
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
                AllValuesEditorControlsStackPanel.Children.Add(editorControl);
                _timelineEditorControls.Add(editorControl);
            }


            // todo: add nested timelines painter + editors
            _playPosPainter = new PlayPosPainter(PlayPosGrid, _viewContext, _playPosSynchronizer);
            _gridPainter = new GridTimePainter(OpticalTimeGrid, _viewContext);

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
            if (_timelineEditorControls != null)
                foreach (UserControl editorControl in _timelineEditorControls)
                    editorControl.Height = ZoomVerticalHeightPerValueEditor;
        }

        public double? GetScrollPosForEditorControl(IAwbObject? awbObject)
        {
            if (awbObject is IServo servo)
                foreach (var child in AllValuesEditorControlsStackPanel.Children)
                {
                    if (child is ServoValueEditorControl servoControl)
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

        private void HighlightActualObject()
        {
            if (_timelineData == null) return;
            if (_viewContext == null) return;
            if (_timelineEditorControls == null) return;

            foreach (UserControl editorControl in _timelineEditorControls)
            {
                if ((editorControl as IAwbObjectControl)?.AwbObject == _viewContext.ActualFocusObject)
                {
                    editorControl.BorderBrush = Brushes.LightSlateGray;
                    editorControl.BorderThickness = new Thickness(4); 
                } else
                {
                    editorControl.BorderBrush =null;
                }
            }
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
                    HighlightActualObject();
                    break;

                default:
                    throw new ArgumentOutOfRangeException($"{nameof(e.ChangeType)}:{e.ChangeType}");
            }
        }
    }
}
// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2026 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Actuators;
using Awb.Core.ActuatorsAndObjects;
using Awb.Core.Player;
using Awb.Core.Project.Various;
using Awb.Core.Services;
using Awb.Core.Sounds;
using Awb.Core.Timelines;
using AwbStudio.TimelineEditing;
using AwbStudio.TimelineValuePainters;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AwbStudio.TimelineControls
{
    public partial class TimelineValuesEditorControl : UserControl, ITimelineEditorControl
    {
        private bool _isInitialized;

        private TimelineData? _timelineData;
        private TimelineViewContext? _viewContext;
        private PlayPosSynchronizer? _playPosSynchronizer;
        private PlayPosPainter? _playPosPainter;
        private GridTimePainter? _gridPainter;
        private List<ITimelineEditorControl>? _timelineEditorControls;
        private List<AbstractValuePainter>? _timelineValuePainters;
        private List<ValueEditorSelectionContainerControl>? _editorContainers;

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
            SizeChanged += TimelineValuesEditorControl_SizeChanged;
            Loaded -= TimelineValuesEditorControl_Loaded;
            Unloaded += OnUnloaded;
        }

        private void TimelineValuesEditorControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AlignEditorContainers();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            SizeChanged -= TimelineValuesEditorControl_SizeChanged;

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

            _editorContainers = new List<ValueEditorSelectionContainerControl>();

            // add nested timelines painter + editors
            var nestedTimelineEditorControl = new NestedTimelinesViewerControl();
            nestedTimelineEditorControl.Init(viewContext, timelineCaptions, playPosSynchronizer, actuatorsService, timelineMetaDataService);
            this.WrapValueEditorInContainerControl(actuatorToEdit: NestedTimelinesFakeObject.Singleton, awbObjectControl: nestedTimelineEditorControl, timelineCaptions);

            // add sound painter + editors
            foreach (var soundPlayerActuator in actuatorsService.SoundPlayers)
            {
                var editorControl = new SoundTimelineEditorControl();
                editorControl.Init(soundPlayer: soundPlayerActuator, viewContext, timelineCaptions, projectSounds: projectSounds);
                this.WrapValueEditorInContainerControl(actuatorToEdit: soundPlayerActuator, awbObjectControl: editorControl, timelineCaptions);
            }

            // add servo painter + editors
            foreach (var servoActuator in actuatorsService.Servos)
            {
                var editorControl = new ServoTimelineEditorControl();
                editorControl.Init(servo: servoActuator, viewContext, timelineCaptions, timelineDataService, projectSounds: projectSounds, awbLogger);
                this.WrapValueEditorInContainerControl(actuatorToEdit: servoActuator, awbObjectControl: editorControl, timelineCaptions);
            }

            _playPosPainter = new PlayPosPainter(PlayPosGrid, _viewContext, _playPosSynchronizer);
            _gridPainter = new GridTimePainter(OpticalTimeGrid, _viewContext);

            UpdateZoom();

            foreach (UserControl editorControl in _timelineEditorControls)
            {
                editorControl.PreviewMouseDown += (sender, e) =>
                {
                    if (e.ChangedButton == MouseButton.Left)
                    {
                        if (_viewContext == null) return;
                        if (_playPosSynchronizer == null) return;
                        var mouseX = e.GetPosition(this).X;
                        var newPlayPosMs = (int)(((mouseX) / _viewContext.PixelPerMs) + PlayPosSynchronizer.SnapMs / 2);
                        _playPosSynchronizer.SetNewPlayPos(newPlayPosMs);
                    }
                };
            }

            AlignEditorContainers();

            _isInitialized = true;
        }

        private void WrapValueEditorInContainerControl(IActuator actuatorToEdit, IAwbObjectControl awbObjectControl, TimelineCaptions timelineCaptions)
        {
            if (_timelineEditorControls == null) throw new InvalidOperationException("Timeline editor controls list is null");
            if (_editorContainers == null) throw new InvalidOperationException("Editor containers list is null");
            if (_viewContext == null) throw new InvalidOperationException("View context is null");
            if (awbObjectControl is not ITimelineEditorControl timelineEditorControl) throw new InvalidOperationException("awbObjectControl is not a ITimelineEditorControl");
            if (awbObjectControl is not UserControl userControl) throw new InvalidOperationException("awbObjectControl is not a UserControl");


            _timelineEditorControls.Add(timelineEditorControl);

            // create a container for the servo timeline editor, so that we can select it and highlight the label when it is selected
            var editorContainer = new ValueEditorSelectionContainerControl();
            editorContainer.Init(awbObjectControl, userControl, _viewContext, timelineCaptions!.GetAktuatorCaption(actuatorToEdit.Id));
            _editorContainers.Add(editorContainer);
            AllValuesEditorControlsStackPanel.Children.Add(editorContainer);
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

            AlignEditorContainers();
        }

        private void UpdateZoom()
        {
            if (_timelineEditorControls != null)
            {
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
                AlignEditorContainers();
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
                    this.Width = newWidth;
                    AlignEditorContainers();
                    break;

                case ViewContextChangedEventArgs.ChangeTypes.Scroll:
                    // set the left margin of the AllValuesEditorControlsStackPanel to the scroll position
                    AlignEditorContainers();
                    break;

                case ViewContextChangedEventArgs.ChangeTypes.BankIndex:
                case ViewContextChangedEventArgs.ChangeTypes.FocusObjectValue:
                    break;

                case ViewContextChangedEventArgs.ChangeTypes.FocusObject:
                    break;

                case ViewContextChangedEventArgs.ChangeTypes.Selection:
                    break;

                default:
                    throw new ArgumentOutOfRangeException($"{nameof(e.ChangeType)}:{e.ChangeType}");
            }
        }

        private void AlignEditorContainers()
        {
            if (_viewContext == null) return;
            foreach (var editorContainer in _editorContainers!)
                editorContainer.Margin = new Thickness(_viewContext.ScrollPositionPx, 0, 0, 0);
            return;
        }
    }
}
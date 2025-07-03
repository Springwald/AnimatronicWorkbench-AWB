// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Actuators;
using Awb.Core.ActuatorsAndObjects;
using Awb.Core.Services;
using Awb.Core.Sounds;
using Awb.Core.Timelines;
using AwbStudio.TimelineEditing;
using AwbStudio.TimelineValuePainters;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AwbStudio.TimelineControls
{
    public partial class ServoTimelineEditorControl : UserControl, ITimelineEditorControl, IAwbObjectControl
    {
        private TimelineViewContext? _viewContext;
        private TimelineCaption? _caption;
        private bool _isInitialized;
        private ServoValuePainter? _servoValuePainter;
        private volatile bool _isDrawing;
        private int _paintCounter = 0;
        private OpticalGridCalculator? _opticalGridCalculator;
        private readonly TimelineColors _timelineColors;

        public IServo? Servo { get; private set; }

        public IAwbObject? AwbObject => Servo;


        public ServoTimelineEditorControl()
        {
            InitializeComponent();
            Loaded += ServoValueViewerControl_Loaded;
            _timelineColors = new TimelineColors();
        }
        private void ServoValueViewerControl_Loaded(object sender, RoutedEventArgs e)
        {
            SizeChanged += ServoValueViewerControl_SizeChanged;
            Unloaded += ServoValueViewerControl_Unloaded;
        }

        private void ServoValueViewerControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Unloaded -= ServoValueViewerControl_Unloaded;
            SizeChanged -= ServoValueViewerControl_SizeChanged;
            if (_servoValuePainter != null)
            {
                _servoValuePainter.Dispose();
                _servoValuePainter = null;
            }
        }

        public void Init(IServo servo, TimelineViewContext viewContext, TimelineCaptions timelineCaptions, ITimelineDataService timelineDataService, Sound[] projectSounds, IAwbLogger awbLogger)
        {
            _viewContext = viewContext;
            Servo = servo;
            _servoValuePainter = new ServoValuePainter(servo, AllValuesGrid, _viewContext, timelineCaptions, timelineDataService, projectSounds: projectSounds, awbLogger, dotRadius: 6);
            _caption = timelineCaptions?.GetAktuatorCaption(servo.Id);
            HeaderControl.TimelineCaption = _caption;
            HeaderControl.MyObject = servo;
            HeaderControl.ViewContext = viewContext;

            _isInitialized = true;
        }

        public void TimelineDataLoaded(TimelineData? timelineData)
        {
            if (!_isInitialized) throw new InvalidOperationException(Name + " not initialized");

            _opticalGridCalculator = new OpticalGridCalculator(OpticalGrid);

            if (timelineData == null) this.Visibility = Visibility.Hidden;
            else
            {
                this.Visibility = Visibility.Visible;
                _servoValuePainter!.TimelineDataLoaded(timelineData);
            }
            DrawOpticalGrid("ServoValueViewerControl_TimelineDataLoaded");
        }

        private void ServoValueViewerControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawOpticalGrid("ServoValueViewerControl_SizeChanged" + e.PreviousSize + " => " + e.NewSize);
        }

        private void DrawOpticalGrid(string reason)
        {
            if (_isDrawing) return;
            if (OpticalGrid.Width < 100 || OpticalGrid.Height < 50) return;

            Debug.WriteLine("DrawOpticalGrid :" + reason + " " + _paintCounter++);

            if (_opticalGridCalculator == null)
            {
                Debug.WriteLine("DrawOpticalGrid: _opticalGridCalculator is null, skipping drawing");
                return;
            }

            _isDrawing = true;

            OpticalGrid.Children.Clear();

            //foreach (var valuePercent in new[] { 0, 25, 50, 75, 100 })
            foreach (var valuePercent in new[] { 0, 25, 50, 75, 100 })
            {
                //var y = height - PaintMarginTopBottom - valuePercent / 100.0 * diagramHeight;
                var y = _opticalGridCalculator.GetYForPercentValue(valuePercent);
                OpticalGrid.Children.Add(new Line
                {
                    X1 = _opticalGridCalculator.PaintGridAreaLeft,
                    X2 = _opticalGridCalculator.PaintGridAreaWidth,
                    Y1 = y,
                    Y2 = y,
                    Stroke = new[] { 0, 50, 100 }.Contains(valuePercent) ? _timelineColors.GridLineHorizontalBrushPrimary : _timelineColors.GridLineHorizontalBrushSecondary
                });
            }

            _isDrawing = false;
        }

        private void Grid_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_viewContext != null && this.Servo != null)
                _viewContext.ActualFocusObject = this.Servo;
        }

    }
}

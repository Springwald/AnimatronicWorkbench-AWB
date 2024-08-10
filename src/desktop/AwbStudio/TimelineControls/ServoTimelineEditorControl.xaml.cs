// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Actuators;
using Awb.Core.ActuatorsAndObjects;
using Awb.Core.Services;
using Awb.Core.Timelines;
using AwbStudio.TimelineEditing;
using AwbStudio.TimelineValuePainters;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AwbStudio.TimelineControls
{
    /// <summary>
    /// Interaction logic for ServoValueViewerControl.xaml
    /// </summary>
    public partial class ServoTimelineEditorControl : UserControl, ITimelineEditorControl, IAwbObjectControl
    {
        private readonly Brush _gridLineBrush = new SolidColorBrush(Color.FromRgb(60, 60, 100));
        private const double _paintMarginTopBottom = 30;

        private TimelineData? _timelineData;
        private TimelineViewContext? _viewContext;

        private TimelineCaption? _caption;
        private bool _isInitialized;
        private ServoValuePainter? _servoValuePainter;

        private string _captionText = string.Empty;

        public IServo? Servo { get; private set; }

        public IAwbObject? AwbObject => Servo;


        public ServoTimelineEditorControl()
        {
            InitializeComponent();
            Loaded += ServoValueViewerControl_Loaded;
        }
        private void ServoValueViewerControl_Loaded(object sender, RoutedEventArgs e)
        {
            DrawOpticalGrid("ServoValueViewerControl_Loaded");
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

        public void Init(IServo servo, TimelineViewContext viewContext, TimelineCaptions timelineCaptions, ITimelineDataService timelineDataService, IAwbLogger awbLogger)
        {
            _viewContext = viewContext;
            Servo = servo;
            _servoValuePainter = new ServoValuePainter(servo, AllValuesGrid, _viewContext, timelineCaptions, timelineDataService, awbLogger, dotRadius: 6);
            _caption = timelineCaptions?.GetAktuatorCaption(servo.Id);
            HeaderControl.TimelineCaption = _caption;
            HeaderControl.ViewContext = viewContext;

            _isInitialized = true;
        }

        public void TimelineDataLoaded(TimelineData? timelineData)
        {
            if (!_isInitialized) throw new InvalidOperationException(Name + " not initialized");
            if (timelineData == null) this.Visibility = Visibility.Hidden;
            else
            {
                this.Visibility = Visibility.Visible;
                _servoValuePainter!.TimelineDataLoaded(timelineData);
                _timelineData = timelineData;
            }
        }

        private DateTime _lastOpticalGridPaintBecauseOfResize = DateTime.MinValue;

        private void ServoValueViewerControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (DateTime.UtcNow - _lastOpticalGridPaintBecauseOfResize < TimeSpan.FromMilliseconds(1000)) return;
            _lastOpticalGridPaintBecauseOfResize = DateTime.UtcNow;
            DrawOpticalGrid("ServoValueViewerControl_SizeChanged" + e.PreviousSize + " => " + e.NewSize);
        }

        private volatile bool _isDrawing;
        private int _paintCounter = 0;

        private void DrawOpticalGrid(string reason)
        {
            if (_isDrawing)  return;

            double height = this.ActualHeight;
            double width = this.ActualWidth;
            if (height < 100 || width < 100) return;

            _isDrawing = true;

            Debug.WriteLine("DrawOpticalGrid :" + reason + " " + _paintCounter++);   

            OpticalGrid.Children.Clear();

            double diagramHeight = height - _paintMarginTopBottom * 2;

            foreach (var valuePercent in new[] { 0, 25, 50, 75, 100 })
            {
                var y = height - _paintMarginTopBottom - valuePercent / 100.0 * diagramHeight;
                OpticalGrid.Children.Add(new Line { X1 = 0, X2 = width, Y1 = y, Y2 = y, Stroke = _gridLineBrush });
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

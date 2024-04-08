// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Actuators;
using Awb.Core.Player;
using Awb.Core.Services;
using Awb.Core.Timelines;
using AwbStudio.TimelineEditing;
using AwbStudio.TimelineValuePainters;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AwbStudio.TimelineControls
{
    /// <summary>
    /// Interaction logic for SoundValueViewerControl.xaml
    /// </summary>
    public partial class SoundValueEditorControl : UserControl, ITimelineEditorControl
    {
        private const double _paintMarginTopBottom = 30;
        private readonly Brush _gridLineBrush = new SolidColorBrush(Color.FromRgb(60, 60, 100));

        private TimelineData? _timelineData;
        private TimelineCaptions? _timelineCaptions;
        private TimelineViewContext? _viewContext;
        private ISoundPlayer? _soundPlayer;
        private SoundValuePainter? _soundValuePainter;
        private TimelineCaption? _caption;
        private bool _isInitialized;

        public SoundValueEditorControl()
        {
            InitializeComponent();
            Loaded += ServoValueViewerControl_Loaded;
        }
        private void ServoValueViewerControl_Loaded(object sender, RoutedEventArgs e)
        {
            DrawOpticalGrid();
            SizeChanged += ServoValueViewerControl_SizeChanged;
            Unloaded += ServoValueViewerControl_Unloaded;
        }

        private void ServoValueViewerControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Unloaded -= ServoValueViewerControl_Unloaded;
            if (_soundValuePainter != null)
            {
                _soundValuePainter.Dispose();
                _soundValuePainter = null;
            }
        }

        public void Init(ISoundPlayer soundPlayer, TimelineViewContext viewContext, TimelineCaptions timelineCaptions, PlayPosSynchronizer playPosSynchronizer, IActuatorsService actuatorsService)
        {
            _viewContext = viewContext;
            _soundPlayer = soundPlayer;
            _soundValuePainter = new SoundValuePainter(soundPlayer, AllValuesGrid, _viewContext, timelineCaptions);
            _caption = timelineCaptions?.GetAktuatorCaption(soundPlayer.Id) ?? new TimelineCaption { ForegroundColor = new SolidColorBrush(Colors.White) };
            HeaderControl.TimelineCaption = _caption;

            _isInitialized = true;
        }

        public void TimelineDataLoaded(TimelineData timelineData)
        {
            if (!_isInitialized) throw new InvalidOperationException(Name + " not initialized");
            _soundValuePainter!.TimelineDataLoaded(timelineData);
            _timelineData = timelineData;
        }

        private void ServoValueViewerControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawOpticalGrid();
        }

        private void DrawOpticalGrid()
        {
            OpticalGrid.Children.Clear();

            double height = this.ActualHeight;
            double width = this.ActualWidth;

            if (height < 100 || width < 100) return;

            double diagramHeight = height - _paintMarginTopBottom * 2;

            foreach (var valuePercent in new[] { 0, 25, 50, 75, 100 })
            {
                var y = height - _paintMarginTopBottom - valuePercent / 100.0 * diagramHeight;
                OpticalGrid.Children.Add(new Line { X1 = 0, X2 = width, Y1 = y, Y2 = y, Stroke = _gridLineBrush });
            }
        }

    }
}

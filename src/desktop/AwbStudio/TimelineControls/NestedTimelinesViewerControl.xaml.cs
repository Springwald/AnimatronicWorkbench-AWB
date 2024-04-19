// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.ActuatorsAndObjects;
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
    /// Interaction logic for NestedTimelinesViewerControl.xaml
    /// </summary>
    public partial class NestedTimelinesViewerControl : UserControl, ITimelineEditorControl, IAwbObjectControl
    {

        private const double _paintMarginTopBottom = 30;
        private readonly Brush _gridLineBrush = new SolidColorBrush(Color.FromRgb(60, 60, 100));

        private TimelineData? _timelineData;
        private TimelineCaptions? _timelineCaptions;
        private TimelineViewContext? _viewContext;
        private NestedTimelineValuePainter? _nestedTimelineValuePainter;
        private TimelineCaption? _caption;
        private bool _isInitialized;

        public IAwbObject? AwbObject => NestedTimelinesFakeObject.Singleton;

        public NestedTimelinesViewerControl()
        {
            InitializeComponent();
            Loaded += ServoValueViewerControl_Loaded;
        }
        private void ServoValueViewerControl_Loaded(object sender, RoutedEventArgs e)
        {
            SizeChanged += ServoValueViewerControl_SizeChanged;
            Unloaded += ServoValueViewerControl_Unloaded;
        }

        private void ServoValueViewerControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Unloaded -= ServoValueViewerControl_Unloaded;
            if (_nestedTimelineValuePainter != null)
            {
                _nestedTimelineValuePainter.Dispose();
                _nestedTimelineValuePainter = null;
            }
        }

        public void Init(TimelineViewContext viewContext, TimelineCaptions timelineCaptions, PlayPosSynchronizer playPosSynchronizer, IActuatorsService actuatorsService)
        {
            _viewContext = viewContext;
            _nestedTimelineValuePainter = new NestedTimelineValuePainter(AllValuesGrid, _viewContext, timelineCaptions);
            _caption = timelineCaptions?.GetAktuatorCaption(NestedTimelinesFakeObject.Singleton.Id) ?? new TimelineCaption { ForegroundColor = new SolidColorBrush(Colors.White) };
            HeaderControl.TimelineCaption = _caption;

            _isInitialized = true;
        }

        public void TimelineDataLoaded(TimelineData timelineData)
        {
            if (!_isInitialized) throw new InvalidOperationException(Name + " not initialized");
            _nestedTimelineValuePainter!.TimelineDataLoaded(timelineData);
            _timelineData = timelineData;
        }

        private void ServoValueViewerControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
        }

        private void Grid_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _viewContext.ActualFocusObject = NestedTimelinesFakeObject.Singleton;
        }
    }
}

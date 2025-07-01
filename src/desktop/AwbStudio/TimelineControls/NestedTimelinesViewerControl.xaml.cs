// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.ActuatorsAndObjects;
using Awb.Core.Player;
using Awb.Core.Project.Various;
using Awb.Core.Services;
using Awb.Core.Timelines;
using AwbStudio.TimelineEditing;
using AwbStudio.TimelineValuePainters;
using System;
using System.Windows;
using System.Windows.Controls;

namespace AwbStudio.TimelineControls
{
    public partial class NestedTimelinesViewerControl : UserControl, ITimelineEditorControl, IAwbObjectControl
    {

        private TimelineViewContext? _viewContext;
        private NestedTimelineValuePainter? _nestedTimelineValuePainter;
        private TimelineCaption? _caption;
        private bool _isInitialized;

        public IAwbObject? AwbObject => NestedTimelinesFakeObject.Singleton;

        public NestedTimelinesViewerControl()
        {
            InitializeComponent();
            Loaded += NestedTimlinesViewerControl_Loaded;
        }
        private void NestedTimlinesViewerControl_Loaded(object sender, RoutedEventArgs e)
        {
            SizeChanged += NestedTimelinesViewerControl_SizeChanged;
            Unloaded += NestedTimelinesViewerControl_Unloaded;
        }

        private void NestedTimelinesViewerControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Unloaded -= NestedTimelinesViewerControl_Unloaded;
            SizeChanged -= NestedTimelinesViewerControl_SizeChanged;
            if (_nestedTimelineValuePainter != null)
            {
                _nestedTimelineValuePainter.Dispose();
                _nestedTimelineValuePainter = null;
            }
        }

        public void Init(TimelineViewContext viewContext, TimelineCaptions timelineCaptions, PlayPosSynchronizer playPosSynchronizer, IActuatorsService actuatorsService, ITimelineMetaDataService timelineMetaDataService)
        {
            _viewContext = viewContext;
            if (_nestedTimelineValuePainter != null)
            {
                _nestedTimelineValuePainter.Dispose();
                _nestedTimelineValuePainter = null;
            }
            _nestedTimelineValuePainter = new NestedTimelineValuePainter(AllValuesGrid, _viewContext, timelineCaptions, timelineMetaDataService);
            _caption = timelineCaptions?.GetAktuatorCaption(NestedTimelinesFakeObject.Singleton.Id);
            HeaderControl.MyObject = NestedTimelinesFakeObject.Singleton;
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
                _nestedTimelineValuePainter!.TimelineDataLoaded(timelineData);
            }
        }

        private void NestedTimelinesViewerControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
        }

        private void Grid_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_viewContext != null) _viewContext.ActualFocusObject = AwbObject;
        }
    }
}

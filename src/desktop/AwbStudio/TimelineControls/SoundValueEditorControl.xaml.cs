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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AwbStudio.TimelineControls
{
    /// <summary>
    /// Interaction logic for SoundValueViewerControl.xaml
    /// </summary>
    public partial class SoundValueEditorControl : UserControl, ITimelineEditorControl, IAwbObjectControl
    {
        private const double _paintMarginTopBottom = 30;
        private readonly Brush _gridLineBrush = new SolidColorBrush(Color.FromRgb(60, 60, 100));

        private TimelineViewContext? _viewContext;
        private ISoundPlayer? _soundPlayer;
        private SoundValuePainter? _soundValuePainter;
        private TimelineCaption? _caption;
        private bool _isInitialized;

        public IAwbObject? AwbObject => _soundPlayer;

        public SoundValueEditorControl()
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
            if (_soundValuePainter != null)
            {
                _soundValuePainter.Dispose();
                _soundValuePainter = null;
            }
        }

        public void Init(ISoundPlayer soundPlayer, TimelineViewContext viewContext, TimelineCaptions timelineCaptions, Sound[] projectSounds)
        {
            _viewContext = viewContext;
            _soundPlayer = soundPlayer;
            _soundValuePainter = new SoundValuePainter(soundPlayer, AllValuesGrid, _viewContext, timelineCaptions, projectSounds);
            _caption = timelineCaptions?.GetAktuatorCaption(soundPlayer.Id) ?? new TimelineCaption { ForegroundColor = new SolidColorBrush(Colors.White) };
            HeaderControl.TimelineCaption = _caption;

            _isInitialized = true;
        }

        public void TimelineDataLoaded(TimelineData timelineData)
        {
            if (!_isInitialized) throw new InvalidOperationException(Name + " not initialized");
            _soundValuePainter!.TimelineDataLoaded(timelineData);
        }

        private void ServoValueViewerControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
        }

        private void Grid_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_viewContext != null && _soundPlayer != null)
                _viewContext.ActualFocusObject = _soundPlayer;
        }
    }
}

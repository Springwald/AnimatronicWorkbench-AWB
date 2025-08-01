﻿// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Actuators;
using Awb.Core.ActuatorsAndObjects;
using Awb.Core.Sounds;
using Awb.Core.Timelines;
using AwbStudio.TimelineEditing;
using AwbStudio.TimelineValuePainters;
using System;
using System.Windows;
using System.Windows.Controls;

namespace AwbStudio.TimelineControls
{
    public partial class SoundTimelineEditorControl : UserControl, ITimelineEditorControl, IAwbObjectControl
    {
        private TimelineViewContext? _viewContext;
        private ISoundPlayer? _soundPlayer;
        private SoundValuePainter? _soundValuePainter;
        private bool _isInitialized;

        public IAwbObject? AwbObject => _soundPlayer;

        public SoundTimelineEditorControl()
        {
            InitializeComponent();
            Loaded += SoundValueViewerControl_Loaded;
        }
        private void SoundValueViewerControl_Loaded(object sender, RoutedEventArgs e)
        {
            SizeChanged += SoundValueViewerControl_SizeChanged;
            Unloaded += SoundValueViewerControl_Unloaded;
        }

        private void SoundValueViewerControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Unloaded -= SoundValueViewerControl_Unloaded;
            SizeChanged -= SoundValueViewerControl_SizeChanged;
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
            _isInitialized = true;
        }

        public void TimelineDataLoaded(TimelineData? timelineData)
        {
            if (!_isInitialized) throw new InvalidOperationException(Name + " not initialized");
            if (timelineData == null) this.Visibility = Visibility.Hidden;
            else
            {
                this.Visibility = Visibility.Visible;
                _soundValuePainter!.TimelineDataLoaded(timelineData);
            }
        }

        private void SoundValueViewerControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
        }

        private void Grid_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_viewContext != null && _soundPlayer != null)
                _viewContext.ActualFocusObject = _soundPlayer;
        }
    }
}

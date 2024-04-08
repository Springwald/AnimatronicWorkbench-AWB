// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Player;
using Awb.Core.Services;
using Awb.Core.Timelines;
using AwbStudio.TimelineEditing;
using System;
using System.Windows;
using System.Windows.Controls;

namespace AwbStudio.TimelineControls
{
    /// <summary>
    /// Interaction logic for SoundValueViewerControl.xaml
    /// </summary>
    public partial class SoundValueEditorControl : UserControl, ITimelineEditorControl
    {

        private TimelineData? _timelineData;
        private TimelineCaptions _timelineCaptions;
        private TimelineViewContext? _viewContext;
        private bool _isInitialized;

        public SoundValueEditorControl()
        {
            InitializeComponent();
            Loaded += SoundValueViewerControl_Loaded;
        }

        public void Init(Awb.Core.Actuators.ISoundPlayer soundPlayer, TimelineViewContext viewContext, TimelineCaptions timelineCaptions, PlayPosSynchronizer playPosSynchronizer, IActuatorsService actuatorsService)
        {
            _viewContext = viewContext;
            _viewContext.Changed += ViewContext_Changed;
            _timelineCaptions = timelineCaptions;
            _isInitialized = true;
        }

        public void TimelineDataLoaded(TimelineData timelineData)
        {
            if (!_isInitialized) throw new InvalidOperationException(Name + " is not initialized!");
            _timelineData = timelineData;
            PaintSoundValues();
        }

        private void SoundValueViewerControl_Loaded(object sender, RoutedEventArgs e)
        {
            SizeChanged += SoundValueViewerControl_SizeChanged;
        }

        private void SoundValueViewerControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
        }

        private void ViewContext_Changed(object? sender, EventArgs e)
        {
            MyInvoker.Invoke(new Action(() => this.PaintSoundValues()));
        }

        private void PaintSoundValues()
        {
            if (!_isInitialized) throw new InvalidOperationException(Name + " not initialized");
            if (_timelineData == null) return;
        }
    }
}

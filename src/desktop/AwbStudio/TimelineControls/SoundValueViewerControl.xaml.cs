// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Services;
using Awb.Core.Sounds;
using Awb.Core.Timelines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AwbStudio.TimelineControls
{
    /// <summary>
    /// Interaction logic for SoundValueViewerControl.xaml
    /// </summary>
    public partial class SoundValueViewerControl : UserControl, ITimelineControl
    {
        public SoundValueViewerControl()
        {
            InitializeComponent();
            this._protoypeLabel = WpfToolbox.XamlClone(PrototypeLabel);
            Loaded += SoundValueViewerControl_Loaded;
        }

        private readonly Label _protoypeLabel;
        private const double _paintMarginTopBottom = 30;

        private TimelineData? _timelineData;
        private TimelineViewPos? _viewPos;

        /// <summary>
        /// The timeline data to be displayed
        /// </summary>
        public TimelineData? TimelineData
        {
            get { return _timelineData; }
            set
            {
                _timelineData = value;
                PaintSoundValues();
            }
        }

        public TimelineCaptions TimelineCaptions { get; set; }

        /// <summary>
        /// The actual view and scroll position of the timeline
        /// </summary>
        public TimelineViewPos ViewPos
        {
            set
            {
                if (value != null)
                {
                    if (_viewPos != null) throw new Exception("ViewPos.Changed is already set");
                    _viewPos = value;
                    _viewPos.Changed += this.OnViewPosChanged;
                    this.OnViewPosChanged(this, EventArgs.Empty);
                }
            }
            get => _viewPos;
        }

        public IActuatorsService? ActuatorsService { set { } }

        private void SoundValueViewerControl_Loaded(object sender, RoutedEventArgs e)
        {
            SizeChanged += SoundValueViewerControl_SizeChanged;
        }

        private void SoundValueViewerControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
        }

        private void OnViewPosChanged(object? sender, EventArgs e)
        {
            MyInvoker.Invoke(new Action(() => this.PaintSoundValues()));
        }

        public void PaintSoundValues()
        {
            CanvasSounds.Children.Clear();

            if (_viewPos == null) return;
            if (_timelineData == null) return;

            double height = this.ActualHeight;
            double width = this.ActualWidth; 

            var soundPlayerIds = _timelineData?.SoundPoints?.OfType<SoundPoint>().Select(p => p.SoundPlayerId).Where(id => id != null).Distinct().ToArray() ?? Array.Empty<string>();

            double diagramHeight = height - _paintMarginTopBottom * 2;

            // Update the content points and lines
            // ToDo: cache and only update on changes; or: use model binding and auto update

            double y = 0;

            foreach (var soundPlayerId in soundPlayerIds)
            {
                var caption = TimelineCaptions?.GetAktuatorCaption(soundPlayerId) ?? new TimelineCaption { ForegroundColor = new SolidColorBrush(Colors.LightYellow) };

                // Add polylines with points
                var pointsForThisSoundplayer = _timelineData?.SoundPoints.OfType<SoundPoint>().Where(p => p.SoundPlayerId == soundPlayerId).OrderBy(p => p.TimeMs).ToList() ?? new List<SoundPoint>();

                // add dots
                foreach (var point in pointsForThisSoundplayer)
                {
                    if (point.TimeMs >= ViewPos.ScrollOffsetMs && point.TimeMs <= ViewPos.DisplayMs + ViewPos.ScrollOffsetMs) // is inside view
                    {
                        var label = WpfToolbox.XamlClone(_protoypeLabel);
                        label.Content = "Sound" + point.Title;
                        label.Foreground = caption.ForegroundColor;
                        label.Background = caption.BackgroundColor;
                        label.Margin = new Thickness { Left = _viewPos.GetXPos(ms: (int)point.TimeMs, controlWidth: width, timelineData: _timelineData) };
                        CanvasSounds.Children.Add(label);
                    }
                }
            }
        }

    }
}

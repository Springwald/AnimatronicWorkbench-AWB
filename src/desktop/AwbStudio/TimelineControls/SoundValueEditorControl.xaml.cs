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
    public partial class SoundValueEditorControl : UserControl, ITimelineEditorControl
    {
        private readonly Label _protoypeLabel;
        private const double _paintMarginTopBottom = 30;

        private TimelineData? _timelineData;
        private TimelineCaptions _timelineCaptions;
        private TimelineViewContext? _viewContext;
        private bool _isInitialized;

        public SoundValueEditorControl()
        {
            InitializeComponent();
            this._protoypeLabel = WpfToolbox.XamlClone(PrototypeLabel);
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

            CanvasSounds.Children.Clear();

            double height = this.ActualHeight;
            double width = this.ActualWidth;

            var soundPlayerIds = _timelineData?.SoundPoints?.OfType<SoundPoint>().Select(p => p.SoundPlayerId).Where(id => id != null).Distinct().ToArray() ?? Array.Empty<string>();

            double diagramHeight = height - _paintMarginTopBottom * 2;

            // Update the content points and lines
            // ToDo: cache and only update on changes; or: use model binding and auto update

            double y = 0;

            foreach (var soundPlayerId in soundPlayerIds)
            {
                var caption = _timelineCaptions?.GetAktuatorCaption(soundPlayerId) ?? new TimelineCaption { ForegroundColor = new SolidColorBrush(Colors.LightYellow) };

                // Add polylines with points
                var pointsForThisSoundplayer = _timelineData?.SoundPoints.OfType<SoundPoint>().Where(p => p.SoundPlayerId == soundPlayerId).OrderBy(p => p.TimeMs).ToList() ?? new List<SoundPoint>();

                // add dots
                foreach (var point in pointsForThisSoundplayer)
                {
                    if (point.TimeMs >= 0 && point.TimeMs <= _viewContext.DurationMs) // is inside view
                    {
                        var label = WpfToolbox.XamlClone(_protoypeLabel);
                        label.Content = "Sound" + point.Title;
                        label.Foreground = caption.ForegroundColor;
                        label.Background = caption.BackgroundColor;
                        label.Margin = new Thickness { Left = _viewContext.GetXPos(timeMs: (int)point.TimeMs, timelineData: _timelineData) };
                        CanvasSounds.Children.Add(label);
                    }
                }
            }
        }

     
    }
}

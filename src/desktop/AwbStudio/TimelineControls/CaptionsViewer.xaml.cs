// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Services;
using System;
using System.Linq;
using System.Windows.Controls;

namespace AwbStudio.TimelineControls
{
    /// <summary>
    /// Interaction logic for CaptionsViewer.xaml
    /// </summary>
    public partial class CaptionsViewer : UserControl
    {
        private IActuatorsService? _actuatorsService;

        public CaptionsViewer()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The service to get the actuators
        /// </summary>
        public IActuatorsService? ActuatorsService
        {
            get => _actuatorsService; set
            {
                if (value != null && _actuatorsService != null) throw new InvalidOperationException("ActuatorsService already set");
                _actuatorsService = value ?? throw new ArgumentNullException(nameof(ActuatorsService));

                // Add servos
                int no = 1;
                foreach (var servo in _actuatorsService.Servos)
                    TimelineCaptions.AddAktuator(servo, $"({no++}) {servo.Label}");

                // Add soudplayer
                foreach (var soundPlayer in _actuatorsService.SoundPlayers)
                    TimelineCaptions.AddAktuator(soundPlayer, $"({no++}) {soundPlayer.Label}");

                LineNames.Children.Clear();
                if (TimelineCaptions?.Captions?.Any() == true)
                {
                    foreach (var caption in TimelineCaptions.Captions)
                    {
                        LineNames.Children.Add(new Label { Content = caption.Label, Foreground = caption.ForegroundColor, Background = caption.BackgroundColor, Opacity = 0.7 });
                    }
                }

            }
        }

        public TimelineCaptions TimelineCaptions { get; set; } = new TimelineCaptions();
    
    }
}

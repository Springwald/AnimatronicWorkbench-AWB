// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Services;
using System;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Xml;

namespace AwbStudio.TimelineControls
{
    /// <summary>
    /// Interaction logic for CaptionsViewer.xaml
    /// </summary>
    public partial class CaptionsViewer : UserControl
    {
        private readonly Label _protoypeLabel;
        private IActuatorsService? _actuatorsService;

        public CaptionsViewer()
        {
            InitializeComponent();
            this._protoypeLabel = XamlClone(PrototypeLabel);
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
                        var label =  XamlClone(PrototypeLabel);
                        label.Content = caption.Label.Trim();
                        label.Foreground = caption.ForegroundColor;
                        label.Background = caption.BackgroundColor;
                        LineNames.Children.Add(label);
                    }
                }
            }
        }

        public TimelineCaptions TimelineCaptions { get; set; } = new TimelineCaptions();

        public T XamlClone<T>(T source)
        {
            string savedObject = System.Windows.Markup.XamlWriter.Save(source);

            // Load the XamlObject
            StringReader stringReader = new StringReader(savedObject);
            System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(stringReader);
            T target = (T)System.Windows.Markup.XamlReader.Load(xmlReader);

            return target;
        }

    }
}

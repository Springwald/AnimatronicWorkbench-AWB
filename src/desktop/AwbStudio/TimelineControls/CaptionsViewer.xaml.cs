// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Services;
using Awb.Core.Timelines;
using System;
using System.Linq;
using System.Windows.Controls;

namespace AwbStudio.TimelineControls
{
    /// <summary>
    /// Interaction logic for CaptionsViewer.xaml
    /// </summary>
    public partial class CaptionsViewer : UserControl, ITimelineControl
    {
        private readonly Border _prototypeLabelBorder;
        private readonly Border _prototypeLabelBorderInActualBank;
        private IActuatorsService? _actuatorsService;
        private TimelineViewPos? _viewPos;
        private int _lastBankIndex;

        public CaptionsViewer()
        {
            InitializeComponent();
            this._prototypeLabelBorder = WpfToolbox.XamlClone(PrototypeLabelBorder);
            this._prototypeLabelBorderInActualBank = WpfToolbox.XamlClone(PrototypeLabelBorderInActualBank);
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

                foreach (var actuator in _actuatorsService.AllActuators)
                    TimelineCaptions.AddAktuator(actuator);
                UpdateCaptionView();
            }
        }

        public TimelineCaptions? TimelineCaptions { get; set; }
        public TimelineData? TimelineData { get; set; }
        public TimelineViewPos? ViewPos
        {
            get => _viewPos;
            set
            {
                _viewPos = value;
                if (_viewPos != null) _viewPos.Changed += ViewPos_Changed;
            }
        }

        private void UpdateCaptionView()
        {
            if (_viewPos == null) return;
            if (_actuatorsService == null) return;



            LineNames.Children.Clear();
            if (TimelineCaptions?.Captions?.Any() == true)
            {
                int no = 0;
                Border? border = null;
                foreach (var caption in TimelineCaptions.Captions)
                {
                    no++;
                    if (no >= _viewPos.FirstBankItemNo && no <= _viewPos.LastBankItemNo)
                    {
                        // actuator is inside the actual bank
                        border = WpfToolbox.XamlClone(_prototypeLabelBorderInActualBank);
                    }
                    else
                    {
                        // actuatator is not inside the actual bank
                        border = WpfToolbox.XamlClone(_prototypeLabelBorder);
                    }

                    var label = border.Child as Label ?? throw new ApplicationException("label border contains no label control?");
                    label.Content = no >= _viewPos.FirstBankItemNo && no <= _viewPos.LastBankItemNo ? $"[{no - _viewPos.FirstBankItemNo + 1}] {caption.Label.Trim()}" : caption.Label.Trim();
                    label.Foreground = caption.ForegroundColor;
                    label.Background = caption.BackgroundColor;
                    LineNames.Children.Add(border);
                }
            }
        }

        private void ViewPos_Changed(object? sender, EventArgs e)
        {
            if (_viewPos == null) return;
            if (_lastBankIndex != _viewPos.BankIndex && _actuatorsService != null)
            {
                _lastBankIndex = _viewPos.BankIndex;
                MyInvoker.Invoke(new Action(() =>
                {
                    this.UpdateCaptionView();
                }));
            }
        }
    }
}

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
using AwbStudio.Tools;
using System;
using System.Linq;
using System.Windows.Controls;

namespace AwbStudio.TimelineControls
{
    /// <summary>
    /// Interaction logic for CaptionsViewer.xaml
    /// </summary>
    public partial class CaptionsViewerControl : UserControl, ITimelineEditorControl
    {
        private readonly Border _prototypeLabelBorder;
        private readonly Border _prototypeLabelBorderInActualBank;

        private IActuatorsService? _actuatorsService;
        private TimelineViewContext? _viewContext;
        private TimelineCaptions? _timelineCaptions;

        private bool _isInitialized;

        public IAwbObject AwbObject => null;

        public CaptionsViewerControl()
        {
            InitializeComponent();
            this._prototypeLabelBorder = WpfToolbox.XamlClone(PrototypeLabelBorder);
            this._prototypeLabelBorderInActualBank = WpfToolbox.XamlClone(PrototypeLabelBorderInActualBank);
        }

        public void Init(TimelineViewContext viewContext, TimelineCaptions timelineCaptions, PlayPosSynchronizer playPosSynchronizer, IActuatorsService actuatorsService)
        {
            _viewContext = viewContext;
            _actuatorsService = actuatorsService;
            _timelineCaptions = timelineCaptions;

            foreach (var actuator in _actuatorsService!.AllActuators)
                _timelineCaptions.AddAktuator(actuator);

            _isInitialized = true;
        }

        public void TimelineDataLoaded(TimelineData timelineData)
        {
            if (!_isInitialized) throw new InvalidOperationException(Name + " not initialized");
            UpdateCaptionView();
        }


        private void UpdateCaptionView()
        {
            if (!_isInitialized) throw new InvalidOperationException(Name + " not initialized");

            if (_timelineCaptions == null) return;

            LineNames.Children.Clear();
            if (_timelineCaptions?.Captions?.Any() == true)
            {
                int no = 0;
                Border? border = null;
                foreach (var caption in _timelineCaptions.Captions)
                {
                    no++;
                    if (no >= _viewContext!.FirstBankItemNo && no <= _viewContext.LastBankItemNo)
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
                    label.Content = no >= _viewContext.FirstBankItemNo && no <= _viewContext.LastBankItemNo ? $"[{no - _viewContext.FirstBankItemNo + 1}] {caption.Label.Trim()}" : caption.Label.Trim();
                    label.Foreground = caption.ForegroundColor;
                    label.Background = caption.BackgroundColor;
                    label.MouseDown += (sender, e) => _viewContext!.ActualFocusObject = _actuatorsService?.AllActuators.SingleOrDefault(a => a.Id == caption.Id);
                    LineNames.Children.Add(border);
                }
            }
        }

    }
}

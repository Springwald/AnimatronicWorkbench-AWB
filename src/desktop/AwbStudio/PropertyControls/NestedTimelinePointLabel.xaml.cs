// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using System.Windows.Controls;

namespace AwbStudio.PropertyControls
{
    /// <summary>
    /// Interaction logic for NestedTimelinePointLabel.xaml
    /// </summary>
    public partial class NestedTimelinePointLabel : UserControl
    {

        public string? LabelText
        {
            get => SoundPlayerLabel.Content?.ToString();
            set
            {
                SoundPlayerLabel.Content = value;
            }
        }

        public void SetWidthByDuration(double widthInPixel)
        {
            SoundPlayerLabel.Width = widthInPixel;
        }   

        public NestedTimelinePointLabel()
        {
            InitializeComponent();
        }
    }
}

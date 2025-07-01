// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using System.Windows.Controls;

namespace AwbStudio.PropertyControls
{
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

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
    /// Interaction logic for SoundPlayerPointLabel.xaml
    /// </summary>
    public partial class SoundPlayerPointLabel : UserControl
    {


        public string? LabelText
        {
            get => SoundPlayerLabel.Content?.ToString();
            set
            {
                SoundPlayerLabel.Content = value;
            }
        }

        public SoundPlayerPointLabel()
        {
            InitializeComponent();
        }
    }
}

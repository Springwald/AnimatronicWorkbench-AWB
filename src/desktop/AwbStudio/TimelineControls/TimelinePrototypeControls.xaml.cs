// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using System.Windows.Controls;
using System.Windows.Shapes;
using AwbStudio.Tools;

namespace AwbStudio.TimelineControls
{
    /// <summary>
    /// Styled prototype controls to instantiate in other controls c# code
    /// </summary>
    public partial class TimelinePrototypeControls : UserControl
    {

        public Line LinePlayPosControl => WpfToolbox.XamlClone(LinePlayPos);

        public Label LabelPlayPosAbsoluteControl => WpfToolbox.XamlClone(LabelPlayPosAbsolute);


        public TimelinePrototypeControls()
        {
            InitializeComponent();
        }
    }
}

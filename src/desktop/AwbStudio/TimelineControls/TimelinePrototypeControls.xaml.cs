﻿// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using AwbStudio.Tools;
using System.Windows.Controls;
using System.Windows.Shapes;

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

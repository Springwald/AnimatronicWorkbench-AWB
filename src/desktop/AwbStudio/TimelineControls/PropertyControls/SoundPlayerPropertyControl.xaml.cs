// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.ActuatorsAndObjects;
using System;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AwbStudio.TimelineControls.PropertyControls
{
    /// <summary>
    /// Interaction logic for SoundPlayerPropertyControl.xaml
    /// </summary>
    public partial class SoundPlayerPropertyControl : UserControl, IPropertyEditor
    {
        public IAwbObject AwbObject { get; }

        public SoundPlayerPropertyControl(IAwbObject awbObject)
        {
            InitializeComponent();
            AwbObject = awbObject;
        }

        public event EventHandler OnValueChanged;

        public async Task UpdateValue()
        {
        }
    }
}

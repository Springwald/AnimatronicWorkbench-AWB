// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using System;
using System.Reflection.Metadata.Ecma335;
using System.Windows.Controls;

namespace AwbStudio.ValueTuning
{
    /// <summary>
    /// Interaction logic for SingleValueControl.xaml
    /// </summary>
    public partial class SingleValueControl : UserControl
    {
        private bool _isSetting = false;

        public SingleValueControl()
        {
            InitializeComponent();
            SliderValue.ValueChanged += SliderValue_ValueChanged;
        }

        private void SliderValue_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isSetting) return;
            Value = e.NewValue;
            ValueChanged?.Invoke(this, e.NewValue);
        }

        public double Value
        {
            get => SliderValue.Value;
            set
            {
                if (SliderValue.Value != value)
                {
                    _isSetting = true;
                    LabelValue.Content = $"{value:0.0}";
                    SliderValue.Value = value;
                    _isSetting = false;
                }
            }
        }

        public string ActuatorName
        {
            set
            {
                LabelName.Content = value;
            }
        }

        public EventHandler<double> ValueChanged;
    }
}

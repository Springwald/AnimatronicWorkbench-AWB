// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using System;
using System.Windows.Controls;

namespace AwbStudio.ValueTuning
{
    /// <summary>
    /// Interaction logic for SingleValueControl.xaml
    /// </summary>
    public partial class NumberValueControl : UserControl
    {
        private volatile bool _isSetting = false;

        public NumberValueControl()
        {
            InitializeComponent();
            SliderValue.ValueChanged += SliderValue_ValueChanged;
        }

        protected override void OnMouseWheel(System.Windows.Input.MouseWheelEventArgs e)
        {
            var newValue = Math.Max(Math.Min(SliderValue.Value + e.Delta / 30d, SliderValue.Maximum), SliderValue.Minimum);
            if (newValue.Equals(SliderValue.Value)) return;
            SliderValue.Value = newValue;
        }

        private void SliderValue_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.NewValue.Equals(e.OldValue)) return;
            if (_isSetting) return;
            ValueChanged?.Invoke(this, e.NewValue);
        }

        public double Value
        {
            get => SliderValue.Value;
            set
            {
                if (value.Equals(Value)) return;
                _isSetting = true;
                LabelValue.Content = $"{value:0.0}";
                SliderValue.Value = value;
                _isSetting = false;
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

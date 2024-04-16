// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Actuators;
using Awb.Core.ActuatorsAndObjects;
using AwbStudio.Tools;
using System;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AwbStudio.TimelineControls.PropertyControls
{
    /// <summary>
    /// Interaction logic for ServoPropertiesControl.xaml
    /// </summary>
    public partial class ServoPropertiesControl : UserControl, IPropertyEditor
    {
        private IServo _servo;
        private bool _isSetting;
        private double _value;
        private double _inverseValue = 1;

        public IAwbObject AwbObject => _servo;

        public ServoPropertiesControl(IServo servo)
        {
            InitializeComponent();
            _servo = servo;
            LabelName.Content = servo.Title;

            if (servo.MinValue > servo.MaxValue)
            {
                // Inverted servo values
                _inverseValue = -1;
                SliderValue.Minimum = _servo.MaxValue;
                SliderValue.Maximum = _servo.MinValue;
            }
            else
            {
                // Normal servo values
                _inverseValue = 1;
                SliderValue.Minimum = _servo.MinValue;
                SliderValue.Maximum =  _servo.MaxValue;
            }
            Loaded += ServoPropertiesControl_Loaded;
        }

        private async void ServoPropertiesControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Loaded -= ServoPropertiesControl_Loaded;
            Unloaded += ServoPropertiesControl_Unloaded;

            SliderValue.ValueChanged += SliderValue_ValueChanged;
            await UpdateValue();
        }

        private void ServoPropertiesControl_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Unloaded -= ServoPropertiesControl_Unloaded;
            SliderValue.ValueChanged -= SliderValue_ValueChanged;
        }

        public async Task UpdateValue()
        {
            Value = _servo.TargetValue;
        }

        protected override void OnMouseWheel(System.Windows.Input.MouseWheelEventArgs e)
        {
            var newValue = Math.Max(Math.Min(SliderValue.Value + e.Delta / 30d, SliderValue.Maximum), SliderValue.Minimum);
            if (newValue.Equals(SliderValue.Value)) return;
            Value = newValue;
        }

        private void SliderValue_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.NewValue.Equals(e.OldValue)) return;
            if (_isSetting) return;
        }

        private double Value
        {
            get => _value;
            set
            {
                if (value.Equals(Value)) return;
                _value = value;
                _isSetting = true;
                
                MyInvoker.Invoke(() =>
                {
                    LabelValue.Content = $"{value:0.0}";
                    if (_inverseValue == -1)
                    {
                        SliderValue.Value = SliderValue.Minimum - value  + SliderValue.Maximum;
                    } else
                    {
                        SliderValue.Value = value;
                    }
                });
                _isSetting = false;
            }
        }
    }
}

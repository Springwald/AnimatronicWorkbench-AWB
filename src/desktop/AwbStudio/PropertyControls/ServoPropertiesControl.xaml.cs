// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Actuators;
using Awb.Core.ActuatorsAndObjects;
using AwbStudio.TimelineEditing;
using System;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AwbStudio.PropertyControls
{
    /// <summary>
    /// Interaction logic for ServoPropertiesControl.xaml
    /// </summary>
    public partial class ServoPropertiesControl : UserControl, IPropertyEditor
    {
        private readonly IServo _servo;
        private readonly TimelineEditingManipulation _timelineEditingManipulation;
        private volatile bool _isSetting;
        private double _percentValue;

        public event EventHandler? OnValueChanged;

        public IAwbObject AwbObject => _servo;

        public ServoPropertiesControl(IServo servo, TimelineEditingManipulation timelineEditingManipulation)
        {
            InitializeComponent();
            _servo = servo;
            _timelineEditingManipulation = timelineEditingManipulation;
            LabelName.Content = "Servo " + servo.Title;
            BtnSetToDefault.Content = $"{servo.PercentCalculator.CalculatePercent(servo.DefaultValue).ToString("0.00")}%";
            SliderValueDefault.Value = servo.PercentCalculator.CalculatePercent(servo.DefaultValue);
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
            PercentValue = _servo.PercentCalculator.CalculatePercent(_servo.TargetValue);
        }

        protected override void OnMouseWheel(System.Windows.Input.MouseWheelEventArgs e)
        {
            var newPercentValue = Math.Max(Math.Min(SliderValue.Value + e.Delta / 30d, SliderValue.Maximum), SliderValue.Minimum);
            if (newPercentValue.Equals(SliderValue.Value)) return;
            PercentValue = newPercentValue;
        }

        private void SliderValue_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.NewValue.Equals(e.OldValue)) return;
            if (_isSetting) return;

            _servo.TargetValue = (int)_servo.PercentCalculator.CalculateValue(e.NewValue);
            PercentValue = e.NewValue;
            OnValueChanged?.Invoke(this, new EventArgs());
        }

        private void BtnSetToDefault_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            PercentValue = _servo.PercentCalculator.CalculatePercent(_servo.DefaultValue);
            _servo.TargetValue = _servo.DefaultValue;
            OnValueChanged?.Invoke(this, new EventArgs());
        }

        private double PercentValue
        {
            get => _percentValue;
            set
            {
                if (value.Equals(_percentValue)) return;
                _percentValue = value;
                _isSetting = true;

                LabelValue.Content = $"{_percentValue:0.00}%";
                SliderValue.Value = value;

                _isSetting = false;
            }
        }

        private void BtnTooglePoint_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _timelineEditingManipulation.ToggleServoPoint(_servo, _percentValue);
        }


    }
}

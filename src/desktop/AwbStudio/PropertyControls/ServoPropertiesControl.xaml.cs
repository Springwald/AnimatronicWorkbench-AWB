// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Actuators;
using Awb.Core.ActuatorsAndObjects;
using Awb.Core.Player;
using Awb.Core.Timelines;
using AwbStudio.TimelineEditing;
using System;
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
        private readonly TimelineData _timelineData;
        private readonly TimelineViewContext _viewContext;
        private readonly PlayPosSynchronizer _playPosSynchronizer;
        private volatile bool _isSetting;
        private double _percentValue;

        public IAwbObject AwbObject => _servo;

        public ServoPropertiesControl(IServo servo, TimelineEditingManipulation timelineEditingManipulation, TimelineData timelineData, TimelineViewContext viewContext, PlayPosSynchronizer playPosSynchronizer)
        {
            InitializeComponent();
            _servo = servo;
            _timelineEditingManipulation = timelineEditingManipulation;
            _timelineData = timelineData;
            _viewContext = viewContext;

            _playPosSynchronizer = playPosSynchronizer;

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
            _playPosSynchronizer.OnPlayPosChanged += OnPlayPosChanged;
            _viewContext.Changed += ViewContext_Changed;
            ShowActualValue();
        }

        private void ServoPropertiesControl_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Unloaded -= ServoPropertiesControl_Unloaded;
            SliderValue.ValueChanged -= SliderValue_ValueChanged;
            _playPosSynchronizer.OnPlayPosChanged -= OnPlayPosChanged;
            _viewContext.Changed -= ViewContext_Changed;
        }

        private void ViewContext_Changed(object? sender, ViewContextChangedEventArgs e)
        {
            switch (e.ChangeType)
            {
                case ViewContextChangedEventArgs.ChangeTypes.FocusObject:
                case ViewContextChangedEventArgs.ChangeTypes.FocusObjectValue:
                    if (_viewContext.ActualFocusObject == _servo)
                        ShowActualValue();
                    break;
            }
        }

        private void OnPlayPosChanged(object? sender, int e)
        {
            if (_viewContext.ActualFocusObject == _servo)
                ShowActualValue();
        }

        protected override void OnMouseWheel(System.Windows.Input.MouseWheelEventArgs e)
        {
            var newPercentValue = Math.Max(Math.Min(SliderValue.Value + e.Delta / 30d, SliderValue.Maximum), SliderValue.Minimum);
            if (newPercentValue.Equals(SliderValue.Value)) return;
            SetNewValue(newPercentValue);
        }

        private void SliderValue_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.NewValue.Equals(e.OldValue)) return;
            if (_isSetting) return;
            SetNewValue(e.NewValue);
        }

        private void BtnSetToDefault_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var percentValue = _servo.PercentCalculator.CalculatePercent(_servo.DefaultValue);
            SetNewValue(percentValue);
        }

        private void BtnTooglePoint_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _timelineEditingManipulation.ToggleServoPoint(_servo, _percentValue);
            ShowActualValue();
        }

        private void SetNewValue(double percentValue)
        {
            if (percentValue.Equals(_percentValue)) return;
            _percentValue = percentValue;
            _isSetting = true;

            if (_servo.TargetValue != (int)_servo.PercentCalculator.CalculateValue(percentValue))
            {
                _servo.TargetValue = (int)_servo.PercentCalculator.CalculateValue(percentValue);
                _viewContext.FocusObjectValueChanged(this);
            }

            ShowActualValue();

            _isSetting = false;
        }

        private void ShowActualValue()
        {
            _percentValue = (int)_servo.PercentCalculator.CalculatePercent(_servo.TargetValue);
            LabelValue.Content = $"{_percentValue:0.00}%";
            SliderValue.Value = _percentValue;
        }




    }
}

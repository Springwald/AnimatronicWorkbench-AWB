// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.InputControllers.TimelineInputControllers;
using AwbStudio.ValueTuning;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace AwbStudio
{
    /// <summary>
    /// Interaction logic for ValueTuningControl.xaml
    /// </summary>
    public partial class ValueTuningWindow : Window, ITimelineController
    {
        private readonly SingleValueControl[] _valuesControls;

        public ValueTuningWindow()
        {
            InitializeComponent();

            _valuesControls = new[]
            {
                ValueControl1,
                ValueControl2,
                ValueControl3,
                ValueControl4,
                ValueControl5,
                ValueControl6,
                ValueControl7,
                ValueControl8
            };

            for (var i = 0; i < _valuesControls.Length; i++)
            {
                var index = i;
                _valuesControls[i].ValueChanged += (sender, value) =>
                {
                    OnTimelineEvent?.Invoke(this, new TimelineControllerEventArgs(TimelineControllerEventArgs.EventTypes.ActuatorValueChanged, actuatorIndex: index, valueInPercent: value));
                };
            }

            KeyDown += ValueTuningWindow_KeyDown;
        }

        private void ValueTuningWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
           switch (e.Key)
            {
                case System.Windows.Input.Key.Tab:
                    OnTimelineEvent?.Invoke(this, new TimelineControllerEventArgs(TimelineControllerEventArgs.EventTypes.NextBank, actuatorIndex: 0, valueInPercent: 0));
                    break;
            }
        }

        public string?[] ActualActuatorNames
        {
            set
            {
                int startAt = 0;
                for (int i = 0; i < _valuesControls.Length; i++)
                {
                    var index = i + startAt;
                    _valuesControls[i].ActuatorName = index >= 0 && index < value.Length ? value[index] : "";
                }
            }
        }

        public event EventHandler<TimelineControllerEventArgs>? OnTimelineEvent;

        public void Dispose()
        {
        }

        public async Task SetActuatorValue(int index, double valueInPercent)
        {
            if (index < 0 || index >= _valuesControls.Length) throw new ArgumentOutOfRangeException($"{nameof(index)}:{index}");
            MyInvoker.Invoke(() =>
            {
                _valuesControls[index].Value = valueInPercent;
            });
            await Task.CompletedTask;
        }

        public async Task SetPlayState(ITimelineController.PlayStates playStates)
        {
            await Task.CompletedTask;
        }

        public Task ShowPointButtonState(int index, bool pointExists)
        {
            return Task.CompletedTask;
        }

        private void TimelineScrollbar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            OnTimelineEvent?.Invoke(this, new TimelineControllerEventArgs(
                        TimelineControllerEventArgs.EventTypes.PlayPosAbsoluteChanged,
                        actuatorIndex: -1,
                        valueInPercent: e.NewValue));
        }
    }
}

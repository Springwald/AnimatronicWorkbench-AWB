﻿// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.InputControllers.TimelineInputControllers;
using Awb.Core.Services;
using AwbStudio.ValueTuning;
using System;
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
        private readonly IActuatorsService _actuatorsService;

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

        public Task SetActuatorValue(int index, double valueInPercent)
        {
            if (index < 0 || index >= _valuesControls.Length) throw new ArgumentOutOfRangeException($"{nameof(index)}:{index}");
            MyInvoker.Invoke(() =>
            {
                _valuesControls[index].Value = valueInPercent;
            });
            return Task.CompletedTask;
        }

        public Task SetPlayState(ITimelineController.PlayStates playStates)
        {
            return Task.CompletedTask;
        }

        public Task ShowPointButtonState(int index, bool pointExists)
        {
            return Task.CompletedTask;
        }
    }
}
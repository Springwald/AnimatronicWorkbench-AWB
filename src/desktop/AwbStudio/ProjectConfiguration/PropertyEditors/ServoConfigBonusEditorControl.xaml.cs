// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Project.Servos;
using Awb.Core.Services;
using System.Windows;
using System.Windows.Controls;

namespace AwbStudio.ProjectConfiguration.PropertyEditors
{
    /// <summary>
    /// Interaction logic for ServoConfigBonusEditor.xaml
    /// </summary>
    public partial class ServoConfigBonusEditorControl : UserControl
    {
        private readonly IAwbClientsService _awbClientsService;

        private IServoConfig _servoConfig;

        public IServoConfig ServoConfig
        {
            get
            {
                return _servoConfig;
            }
            init
            {
                _servoConfig = value;

                // Enable/disable the read position button
                ButtonReadPosition.Visibility = _servoConfig.CanReadServoPosition ? Visibility.Visible : Visibility.Collapsed;

                // set the max / min values for the slider
                if (_servoConfig is FeetechBusServoConfig servo)
                {
                    SliderServoPosition.Minimum = servo.MinValue;
                    SliderServoPosition.Maximum = servo.MaxValue;

                    // show the default values
                    if (servo.DefaultValue is null)
                    {
                        // if the default value is not set, set the slider to the center value
                        var centerValue = servo.MinValue + (servo.MaxValue - servo.MinValue) / 2;
                        SliderServoPosition.Value = centerValue;
                    }
                    else
                        SliderServoPosition.Value = servo.DefaultValue.Value;
                }
                else
                {
                    // hide the sliders if the servo config is not a FeetechBusServoConfig
                    SliderServoPosition.Visibility = Visibility.Collapsed;
                }
            }
        }

        public ServoConfigBonusEditorControl(IAwbClientsService awbClientsService)
        {
            _awbClientsService = awbClientsService;
            InitializeComponent();
        }

        private void ButtonReadPosition_Click(object sender, RoutedEventArgs e)
        {
            if (ServoConfig is null)
            {
                MessageBox.Show("No servo config selected.");
                return;
            }

            if (ServoConfig.CanReadServoPosition == false)
            {
                MessageBox.Show("This servo type does not support reading the position.");
                return;
            }

            if (ServoConfig is FeetechBusServoConfig servo)
            {
                var client = _awbClientsService.GetClient(servo.ClientId);
                if (client == null)
                {
                    MessageBox.Show($"Client {servo.ClientId} not found.");
                    return;
                }
            }

            MessageBox.Show("This servo type does not support reading the position.");
        }
    }
}

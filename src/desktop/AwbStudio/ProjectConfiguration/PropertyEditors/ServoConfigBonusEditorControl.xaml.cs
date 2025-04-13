// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Actuators;
using Awb.Core.Clients;
using Awb.Core.DataPackets;
using Awb.Core.DataPackets.ResponseDataPackets;
using Awb.Core.Project.Servos;
using Awb.Core.Services;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
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
                    LabelMinValue.Content = servo.MinValue.ToString();

                    SliderServoPosition.Maximum = servo.MaxValue;
                    LabelMaxValue.Content = servo.MaxValue.ToString();

                    // show the default values
                    if (servo.DefaultValue is null)
                    {
                        // if the default value is not set, set the slider to the center value
                        var centerValue = servo.MinValue + (servo.MaxValue - servo.MinValue) / 2;
                        SliderServoPosition.Value = centerValue;
                        LabelValue.Content = centerValue.ToString();
                    }
                    else
                    {
                        SliderServoPosition.Value = servo.DefaultValue.Value;
                        LabelValue.Content = servo.DefaultValue.Value.ToString();
                    }
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

        private async void ButtonReadPosition_Click(object sender, RoutedEventArgs e)
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

            if (ServoConfig is FeetechBusServoConfig feetechBusServoConfig)
            {
                var value = await ReadPosFromServo(feetechBusServoConfig);
                if (value.HasValue)
                {
                    LabelValue.Content = value;
                    SliderServoPosition.Value = value.Value;
                }
                return;
            }

            MessageBox.Show("This servo type does not support reading the position.");
        }

        private async Task<int?> ReadPosFromServo(FeetechBusServoConfig servoConfig)
        {
            var dataPacketFactory = new DataPacketFactory();
            var packet = dataPacketFactory.GetDataPacketGetServoPos(servoConfig);

            if (packet == null || packet.IsEmpty)
            {
                MessageBox.Show("Unable to create data packet.");
                return null;
            }

            var clientID = packet.ClientId;
            var client = _awbClientsService.GetClient(clientID);
            if (client == null)
            {
                MessageBox.Show($"ClientId '{clientID}' not found!");
                return null;
            }

            var options = new JsonSerializerOptions()
            {
                WriteIndented = false,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
            var jsonStr = JsonSerializer.Serialize(packet.Content, options);
            var payload = Encoding.ASCII.GetBytes(jsonStr);
            var result = await client.Send(payload);
            if (result.Ok)
            {
                var resultPayloadJsonStr = result.ResultPayload;
                var resultDataPacket = JsonSerializer.Deserialize<ReadValueResponseDataPacket>(resultPayloadJsonStr, options);
                if (resultDataPacket == null)
                {
                    MessageBox.Show($"Unable to read position from servo. Result data packet is null:" + resultPayloadJsonStr);
                    return null;
                }

                if (servoConfig is ScsFeetechServoConfig)
                {
                    if (resultDataPacket.ScsServo == null)
                    {
                        MessageBox.Show($"Unable to read position from servo. Result data packet ScsServo is null:" + resultPayloadJsonStr);
                        return null;
                    }
                    return resultDataPacket.ScsServo.Position;
                }
                if (servoConfig is StsFeetechServoConfig)
                {
                    if (resultDataPacket.StsServo == null)
                    {
                        MessageBox.Show($"Unable to read position from servo. Result data packet ScsServo is null:" + resultPayloadJsonStr);
                        return null;
                    }
                    return resultDataPacket.StsServo.Position;
                }

                MessageBox.Show($"Unable to read position from servo. Result data packet is not a ScsFeetechServoConfig or StsFeetechServoConfig:" + resultPayloadJsonStr);
                return null;
            }
            else
            {
                MessageBox.Show($"Error sending request  to client Id '{clientID}': {result.ErrorMessage}");
                return null;
            }
        }
    }
}

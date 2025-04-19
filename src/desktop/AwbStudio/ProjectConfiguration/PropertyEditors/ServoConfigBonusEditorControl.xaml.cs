// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

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

        private DateTime _lastError = DateTime.MinValue;

        private int? _newSendPositionValue = null;
        private volatile bool _sendingValueToServo = false;

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
                var maxValue = 0;
                var minValue = 0;
                int? defaultValue = null;

                // get the maximum values from "range" property annotation of the attribute FeetechBusServoConfig.MaxValue
                if (_servoConfig is ScsFeetechServoConfig scsFeetechServoConfig)
                {
                    maxValue = ScsFeetechServoConfig.MaxValConst;
                    defaultValue = scsFeetechServoConfig.DefaultValue;
                }
                else if (_servoConfig is StsFeetechServoConfig stsFeetechServoConfig)
                {
                    maxValue = StsFeetechServoConfig.MaxValueConst;
                    defaultValue = stsFeetechServoConfig.DefaultValue;
                }
                else if (_servoConfig is Pca9685PwmServoConfig pca9685PwmServoConfig)
                {
                    maxValue = Pca9685PwmServoConfig.MaxValConst;
                    defaultValue = pca9685PwmServoConfig.DefaultValue;
                }
                else
                {
                    MessageBox.Show("Servo config is not a FeetechBusServoConfig or Pca9685PwmServoConfig.");
                    // hide the sliders if the servo config is not a supported type
                    SliderServoPosition.Visibility = Visibility.Collapsed;
                    LabelMaxValue.Content = "?!?";
                    LabelMinValue.Content = "?!?";
                    LabelValue.Content = "?!?";
                    return;
                }

                SliderServoPosition.Maximum = maxValue;
                LabelMaxValue.Content = maxValue.ToString();

                SliderServoPosition.Minimum = minValue;
                LabelMinValue.Content = minValue.ToString();

                // show the default values
                if (defaultValue is null)
                {
                    // if the default value is not set, set the slider to the center value
                    var centerValue = minValue + (maxValue - minValue) / 2;
                    SliderServoPosition.Value = centerValue;
                    LabelValue.Content = centerValue.ToString();
                }
                else
                {
                    SliderServoPosition.Value = defaultValue.Value;
                    LabelValue.Content = defaultValue.Value.ToString();
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

            var value = await ReadPosFromServo(ServoConfig);
            if (value.HasValue)
            {
                LabelValue.Content = value;
                SliderServoPosition.Value = value.Value;
            }
            return;
        }

        public async Task<int?> ReadPosFromServo(IServoConfig servoConfig)
        {
            var dataPacketFactory = new DataPacketFactory();
            var packet = dataPacketFactory.GetDataPacketGetServoPos(servoConfig);

            if (packet == null || packet.IsEmpty)
            {
                ShowError("Unable to create data packet.");
                return null;
            }

            var clientID = packet.ClientId;
            var client = _awbClientsService.GetClient(clientID);
            if (client == null)
            {
                ShowError($"ClientId '{clientID}' not found!");
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

                ReadValueResponseDataPacket? resultDataPacket = null;
                try
                {
                    resultDataPacket = JsonSerializer.Deserialize<ReadValueResponseDataPacket>(resultPayloadJsonStr, options);
                }
                catch (JsonException ex)
                {
                    ShowError($"Unable to read position from servo. Error deserializing result payload: {ex.Message} - Payload: {resultPayloadJsonStr}");
                    return null;
                }
                if (resultDataPacket == null)
                {
                    ShowError($"Unable to read position from servo. Result data packet is null:" + resultPayloadJsonStr);
                    return null;
                }

                if (servoConfig is ScsFeetechServoConfig)
                {
                    if (resultDataPacket.ScsServo == null)
                    {
                        ShowError($"Unable to read position from servo. Result data packet ScsServo is null:" + resultPayloadJsonStr);
                        return null;
                    }
                    return resultDataPacket.ScsServo.Position;
                }
                if (servoConfig is StsFeetechServoConfig)
                {
                    if (resultDataPacket.StsServo == null)
                    {
                        ShowError($"Unable to read position from servo. Result data packet ScsServo is null:" + resultPayloadJsonStr);
                        return null;
                    }
                    return resultDataPacket.StsServo.Position;
                }

                ShowError($"Unable to read position from servo. Result data packet is not a ScsFeetechServoConfig or StsFeetechServoConfig:" + resultPayloadJsonStr);
                return null;
            }
            else
            {
                ShowError($"Error sending request  to client Id '{clientID}': {result.ErrorMessage}");
                return null;
            }
        }

        private async void SliderServoPosition_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CheckboxSendChangesToServo.IsChecked == true)
            {
                var absolutePosition = (int)e.NewValue;
                _newSendPositionValue = absolutePosition;
                await this.SendValueToServo(ServoConfig, absolutePosition);
            }
            else
            {
                LabelValue.Content = ((int)e.NewValue).ToString();
            }
        }

        private async Task SendValueToServo(IServoConfig servoConfig, int absolutePosition)
        {
            if (_sendingValueToServo) return;
            _sendingValueToServo = true;

            while (true)
            {
                var dataPacketFactory = new DataPacketFactory();
                var packet = dataPacketFactory.GetDataPacketSetServoPos(servoConfig, absolutePosition);
                if (packet == null || packet.IsEmpty)
                {
                    ShowError("Unable to create data packet.");
                    return;
                }
                var clientID = packet.ClientId;
                var client = _awbClientsService.GetClient(clientID);
                if (client == null)
                {
                    ShowError($"ClientId '{clientID}' not found!");
                    return;
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
                    // set the label for the slider to the new value
                    LabelValue.Content = absolutePosition.ToString();
                }
                else
                {
                    ShowError($"Error sending servo pos to client Id '{clientID}': {result.ErrorMessage}");
                }

                if (_newSendPositionValue.HasValue == false || absolutePosition == _newSendPositionValue)
                {
                    // if the value is not changed, break the loop
                    break;
                }
                else
                {
                    // if the value is changed, wait for 100ms and send the new value
                    absolutePosition = _newSendPositionValue.Value;
                    _newSendPositionValue = null;
                    await Task.Delay(100);
                }
            }
            _sendingValueToServo = false;
        }

        /// <summary>
        /// Shows an error message if the last error was more than 5 seconds ago.
        /// </summary>
        private void ShowError(string message)
        {
            var diff = DateTime.UtcNow - _lastError;
            if (diff.TotalSeconds < 5) return;
            _lastError = DateTime.UtcNow;
            MessageBox.Show(message);
        }
    }
}
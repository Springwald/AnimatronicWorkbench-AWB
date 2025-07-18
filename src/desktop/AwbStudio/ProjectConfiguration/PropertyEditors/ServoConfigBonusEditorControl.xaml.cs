﻿// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.DataPackets;
using Awb.Core.DataPackets.ResponseDataPackets;
using Awb.Core.Project.Servos;
using Awb.Core.Services;
using Awb.Core.Tools;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;

namespace AwbStudio.ProjectConfiguration.PropertyEditors
{
    public partial class ServoConfigBonusEditorControl : UserControl
    {
        private readonly IAwbClientsService _awbClientsService;
        private readonly IInvoker _invoker;
        private IServoConfig? _servoConfig;

        private volatile bool _isMoving = false;

        private DateTime _lastError = DateTime.MinValue;
        private string _lastErrorMessage = string.Empty;

        private int? _newSendPositionValue = null;
        private volatile bool _sendingValueToServo = false;

        private Timer _autoPlayTimer = new(5000);
        private Timer _errorMsgTimer = new(500);
        private bool _autoPlayFlipFlop = false;
        private int _maxProjectLimitValue;
        private int _minProjectLimitValue;

        public required IServoConfig? ServoConfig
        {
            get
            {
                return _servoConfig;
            }
            init
            {
                if (value == null) throw new ArgumentNullException("ServoConfig", "ServoConfig cannot be null.");

                _servoConfig = value;

                // Enable/disable the read position button
                ButtonReadPosition.Visibility = _servoConfig.CanReadServoPosition ? Visibility.Visible : Visibility.Collapsed;
                var maxPhysValue = 0;
                var minPhysValue = 0;
                int? defaultValue = null;

                // get the maximum values from "range" property annotation of the attribute FeetechBusServoConfig.MaxValue
                if (_servoConfig is ScsFeetechServoConfig scsFeetechServoConfig)
                {
                    maxPhysValue = ScsFeetechServoConfig.MaxValConst;
                    defaultValue = scsFeetechServoConfig.DefaultValue;
                }
                else if (_servoConfig is StsFeetechServoConfig stsFeetechServoConfig)
                {
                    maxPhysValue = StsFeetechServoConfig.MaxValueConst;
                    defaultValue = stsFeetechServoConfig.DefaultValue;
                }
                else if (_servoConfig is Pca9685PwmServoConfig pca9685PwmServoConfig)
                {
                    maxPhysValue = Pca9685PwmServoConfig.MaxValConst;
                    defaultValue = pca9685PwmServoConfig.DefaultValue;
                }
                else
                {
                    MessageBox.Show("Servo config is not a FeetechBusServoConfig or Pca9685PwmServoConfig.");
                    // hide the sliders if the servo config is not a supported type
                    SliderServoPhysPosition.Visibility = Visibility.Collapsed;
                    LabelPhysMaxValue.Content = "?!?";
                    LabelPhysMinValue.Content = "?!?";
                    LabelPhysValue.Content = "?!?";

                    SliderServoLimitPosition.Visibility = Visibility.Collapsed;
                    LabelLimitMaxValue.Content = "?!?";
                    LabelLimitMinValue.Content = "?!?";
                    LabelLimitValue.Content = "?!?";

                    return;
                }

                // show the sliders if the servo config is a supported type
                SliderServoPhysPosition.Minimum = minPhysValue;
                SliderServoPhysPosition.Maximum = maxPhysValue;
                LabelPhysMinValue.Content = minPhysValue.ToString();
                LabelPhysMaxValue.Content = maxPhysValue.ToString();

                // show the default values
                if (defaultValue is null)
                {
                    // if the default value is not set, set the slider to the center value
                    var centerValue = minPhysValue + (maxPhysValue - minPhysValue) / 2;
                    SliderServoPhysPosition.Value = centerValue;
                    LabelPhysValue.Content = centerValue.ToString();
                }
                else
                {
                    SliderServoPhysPosition.Value = defaultValue.Value;
                    LabelPhysValue.Content = defaultValue.Value.ToString();
                }

                UpdateProjectLimits();
            }
        }

        public ServoConfigBonusEditorControl(IAwbClientsService awbClientsService, IInvoker invoker)
        {
            _awbClientsService = awbClientsService;
            _invoker = invoker;
            Loaded += ServoConfigBonusEditorControl_Loaded;
            InitializeComponent();
        }

        private void ServoConfigBonusEditorControl_Loaded(object sender, RoutedEventArgs e)
        {
            Unloaded += ServoConfigBonusEditorControl_Unloaded;

            _autoPlayTimer.Stop();
            _autoPlayTimer.AutoReset = true;
            _autoPlayTimer.Elapsed += AutoPlayTimer_Elapsed;

            _errorMsgTimer.Elapsed += (s, args) =>
            {
                _invoker.Invoke(() => UpdateErrorMessageVisibility());

            };
            _errorMsgTimer.Start();
        }

        private void UpdateErrorMessageVisibility()
        {
            if (_lastError == DateTime.MinValue)
            {
                labelErrorMsg.Visibility = Visibility.Collapsed;
                return;
            }
            var diff = DateTime.UtcNow - _lastError;
            if (diff.TotalSeconds < 5)
            {
                labelErrorMsg.Visibility = Visibility.Visible;
                labelErrorMsg.Text = _lastErrorMessage + $" ({diff.TotalSeconds:0.0}s ago)";
            }
            else
            {
                labelErrorMsg.Visibility = Visibility.Collapsed;
            }
        }

        private void ServoConfigBonusEditorControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _autoPlayTimer.Stop();
            _errorMsgTimer.Stop();
        }

        public void UpdateProjectLimits()
        {
            _maxProjectLimitValue = 0;
            _minProjectLimitValue = 0;

            if (_servoConfig is ScsFeetechServoConfig scsFeetechServoConfig)
            {
                _minProjectLimitValue = scsFeetechServoConfig.MinValue;
                _maxProjectLimitValue = scsFeetechServoConfig.MaxValue;
            }
            else if (_servoConfig is StsFeetechServoConfig stsFeetechServoConfig)
            {
                _minProjectLimitValue = stsFeetechServoConfig.MinValue;
                _maxProjectLimitValue = stsFeetechServoConfig.MaxValue;
            }
            else if (_servoConfig is Pca9685PwmServoConfig pca9685PwmServoConfig)
            {
                _minProjectLimitValue = pca9685PwmServoConfig.MinValue;
                _maxProjectLimitValue = pca9685PwmServoConfig.MaxValue;
            }

            if (_minProjectLimitValue > _maxProjectLimitValue) // swap the values if they are in the wrong order
                (_minProjectLimitValue, _maxProjectLimitValue) = (_maxProjectLimitValue, _minProjectLimitValue);

            SliderServoLimitPosition.Minimum = _minProjectLimitValue;
            SliderServoLimitPosition.Maximum = _maxProjectLimitValue;
            LabelLimitMinValue.Content = _minProjectLimitValue.ToString();
            LabelLimitMaxValue.Content = _maxProjectLimitValue.ToString();
        }

        /// <summary>
        /// Read the servo position from the servo from the client hardware and update the sliders and labels.
        /// </summary>
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
                LabelPhysValue.Content = value;
                SliderServoPhysPosition.Value = value.Value;

                LabelLimitValue.Content = value;
                SliderServoLimitPosition.Value = value.Value;
            }
            return;
        }


        /// <summary>
        /// Reads the position from the servo using the client hardware packet protocol 
        /// </summary>
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
            var result = await client.Send(payload: jsonStr, debugInfo: jsonStr);
            if (result.Ok)
            {
                var resultPayloadJsonStr = result.ResultPayload;

                if (string.IsNullOrWhiteSpace(resultPayloadJsonStr))
                {
                    ShowError($"Unable to read position from servo. Result payload is empty for client Id '{clientID}'");
                    return null;
                }

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
            if (_isMoving) return; // prevent recursive calls

            if (CheckboxSendChangesToServo.IsChecked == true)
            {
                var absolutePosition = (int)e.NewValue;
                _newSendPositionValue = absolutePosition;
                if (ServoConfig == null)
                {
                    ShowError("ServoConfig is null. Cannot send position to servo.");
                    return;
                }
                await this.SendValueToServo(ServoConfig, absolutePosition);
            }

            await ShowActualPosition(sender, (int)e.NewValue);
        }

        private async Task ShowActualPosition(object sender, int position)
        {
            _isMoving = true;
            if (sender != SliderServoPhysPosition) SliderServoPhysPosition.Value = position;
            if (sender != SliderServoLimitPosition) SliderServoLimitPosition.Value = position;
            LabelPhysValue.Content = (position).ToString();
            LabelLimitValue.Content = (position).ToString();
            await Task.CompletedTask;
            _isMoving = false;
        }

        private async Task SendValueToServo(IServoConfig servoConfig, int absolutePosition)
        {
            if (_sendingValueToServo) return;
            _sendingValueToServo = true;

            while (true)
            {
                var dataPacketFactory = new DataPacketFactory();
                var packet = dataPacketFactory.GetDataPacketSetSingleServoPos(servoConfig, absolutePosition);
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
                var result = await client.Send(payload: jsonStr, debugInfo: jsonStr);
                if (result.Ok)
                {
                    // set the label for the slider to the new value
                    LabelPhysValue.Content = absolutePosition.ToString();
                    LabelLimitValue.Content = absolutePosition.ToString();
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
            _lastErrorMessage = message;
        }

        #region Automove
        private void AutoPlayTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            FlipFlopAutoPlay();
        }

        private void FlipFlopAutoPlay()
        {
            _invoker.Invoke(async () =>
            {
                if (ServoConfig == null)
                {
                    ShowError("ServoConfig is null. Cannot send position to servo.");
                    return;
                }
                var newPos = _autoPlayFlipFlop ? _maxProjectLimitValue : _minProjectLimitValue;
                await SendValueToServo(ServoConfig, newPos);
                await ShowActualPosition(this, newPos);
                _autoPlayFlipFlop = !_autoPlayFlipFlop;
            });
        }

        private void CheckboxAutomove_Checked(object sender, RoutedEventArgs e)
        {
            UpdateAutoPlayDuration();
            _autoPlayFlipFlop = false;
            _autoPlayTimer.Start();
            FlipFlopAutoPlay();
        }

        private void CheckboxAutomove_Unchecked(object sender, RoutedEventArgs e)
        {
            _autoPlayTimer.Stop();
        }

        private void ComboBoxAutomoveDelay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateAutoPlayDuration();
        }

        private void UpdateAutoPlayDuration()
        {
            int delaySeconds = 5;
            if (ComboBoxAutomoveDelay.SelectedItem is ComboBoxItem selectedItem)
            {
                if (int.TryParse(selectedItem.Tag.ToString(), out var selectedDelaySeconds))
                {
                    if (selectedDelaySeconds > 0)
                        delaySeconds = selectedDelaySeconds;
                }
            }
            _autoPlayTimer.Interval = delaySeconds * 1000;
        }
    }

    #endregion // automove
}
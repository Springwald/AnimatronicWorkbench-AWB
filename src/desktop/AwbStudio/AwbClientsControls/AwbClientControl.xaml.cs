// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AwbStudio.AwbClientsControls
{
    /// <summary>
    /// Interaction logic for AwbClientControl.xaml
    /// </summary>
    public partial class AwbClientControl : UserControl
    {
        private IAwbClient? _awbClient;

        public AwbClientControl()
        {
            InitializeComponent();
            Unloaded += AwbClientControl_Unloaded;
        }

        internal void ShowClientData(IAwbClient client)
        {
            this.LabelTitle.Content = $"AWB Client ID {client.ClientId} ({client.FriendlyName})";
            UnbindClient();
            BindClient(client);
        }

        private void AwbClient_Received(object? sender, IAwbClient.ReceivedEventArgs e) => AddLineToDebugLog($"💬 {e.Payload}");

        private void AwbClient_OnError(object? sender, string errorMsg) => AddLineToDebugLog($"⛔ {errorMsg}");

        private void AddLineToDebugLog(string message)
        {
            this.TextBlockDebugLog.Text = $"{message}{Environment.NewLine}{this.TextBlockDebugLog.Text}";
        }

        private void AwbClientControl_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            UnbindClient();
        }

        private void BindClient(IAwbClient client)
        {
            _awbClient = client;
            _awbClient.OnError += AwbClient_OnError;
            _awbClient.Received += AwbClient_Received;
        }

        private void UnbindClient()
        {
            if (_awbClient != null)
            {
                _awbClient.OnError -= AwbClient_OnError;
                _awbClient = null;
            }
        }
    }
}

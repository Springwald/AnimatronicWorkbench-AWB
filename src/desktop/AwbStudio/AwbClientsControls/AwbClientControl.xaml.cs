// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Clients;
using Awb.Core.Clients.Models;
using System;
using System.Threading;
using System.Windows.Controls;

namespace AwbStudio.AwbClientsControls
{
    /// <summary>
    /// Interaction logic for AwbClientControl.xaml
    /// </summary>
    public partial class AwbClientControl : UserControl
    {
        private Timer? _updateClientInformationTimer;
        private IAwbClient? _awbClient;

        public AwbClientControl()
        {
            InitializeComponent();
            Unloaded += AwbClientControl_Unloaded;
            _updateClientInformationTimer = new Timer(UpdateClientInformation, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        }

      

        internal void SetClient(IAwbClient client)
        {
            UnbindClient();
            BindClient(client);
            UpdateClientInformation(null);
        }

        private void AwbClient_Received(object? sender, ReceivedEventArgs e) => AddLineToDebugLog($"💬 {e.Payload}");

        private void AwbClient_OnError(object? sender, string errorMsg) => AddLineToDebugLog($"⛔ {errorMsg}");

        private void AddLineToDebugLog(string message)
        {
            this.TextBlockDebugLog.Text = $"{message}{Environment.NewLine}{this.TextBlockDebugLog.Text}";
        }

        private void AwbClientControl_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_updateClientInformationTimer != null)
            {
                _updateClientInformationTimer.Dispose();
                _updateClientInformationTimer = null;
            }
            UnbindClient();
        }


        private void UpdateClientInformation(object? state)
        {
            // call the update method in the UI thread
            if (this.Dispatcher.CheckAccess())
            {
                UpdateClientInformationInternal();
            }
            else
            {
                this.Dispatcher.Invoke(UpdateClientInformationInternal);
            }
        }

        private void UpdateClientInformationInternal()
        {
            if (_awbClient == null) return;
            this.LabelTitle.Content = $"AWB Client ID {_awbClient.ClientId} ({_awbClient.FriendlyName})";

            var sinceLastError = DateTime.UtcNow - (_awbClient.LastErrorUtc ?? DateTime.MinValue);
            var maxErrorSeconds = 5;
            if (sinceLastError < TimeSpan.FromSeconds(maxErrorSeconds))
            {
                this.LabelStatus.Content = $"🚨 {maxErrorSeconds-sinceLastError.TotalSeconds:0}";
            }
            else
            {
                this.LabelStatus.Content = $"✅";
            }
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

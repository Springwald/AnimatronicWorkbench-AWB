// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Clients;
using Awb.Core.Clients.Models;
using Awb.Core.Tools;
using System;
using System.Threading;
using System.Windows.Controls;

namespace AwbStudio.AwbClientsControls
{
    /// <summary>
    /// Show the status and communication of a single AWB client
    /// </summary>
    public partial class AwbClientControl : UserControl
    {
        private Timer? _updateClientInformationTimer;
        private IAwbClient? _awbClient;
        private readonly IInvokerService _invokerService;

        public AwbClientControl(IInvokerService invokerService)
        {
            _invokerService = invokerService;
            InitializeComponent();

            Loaded += AwbClientControl_Loaded;

            Unloaded += AwbClientControl_Unloaded;
            _updateClientInformationTimer = new Timer(UpdateClientInformation, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        }

        private void AwbClientControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            LabelStatus.Text = "Loading...";
            LabelTitle.Text = "Loading...";
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
            _invokerService.GetInvoker().Invoke(() => UpdateClientInformationInternal());
        }

        private void UpdateClientInformationInternal()
        {
            if (_awbClient == null) return;
            this.LabelTitle.Text = $"AWB Client ID {_awbClient.ClientId} ({_awbClient.FriendlyName})";

            var sinceLastError = DateTime.UtcNow - (_awbClient.LastErrorUtc ?? DateTime.MinValue);
            var maxErrorSeconds = 5;
            if (sinceLastError < TimeSpan.FromSeconds(maxErrorSeconds))
            {
                this.LabelStatus.Text = $"🚨 {maxErrorSeconds - sinceLastError.TotalSeconds:0}";
            }
            else
            {
                this.LabelStatus.Text = $"✅";
            }
        }

        private void BindClient(IAwbClient client)
        {
            _awbClient = client;
            _awbClient.OnError += AwbClient_OnError;
            _awbClient.Received += AwbClient_Received;
            _awbClient.PacketSending += AwbClient_PacketSending;
        }

        private void UnbindClient()
        {
            if (_awbClient != null)
            {
                _awbClient.OnError -= AwbClient_OnError;
                _awbClient.Received -= AwbClient_Received;
                _awbClient.PacketSending -= AwbClient_PacketSending;
                _awbClient = null;
            }
        }

        private void AwbClient_PacketSending(object? sender, string e)
        {
            const int maxLength = 5000;
            var old = this.TextBlockDebugLog.Text ?? string.Empty;
            if (old.Length > maxLength)
                old = old.Substring(0, maxLength);
            var debugInfos = $"----------------------------{Environment.NewLine}📦 {e}{Environment.NewLine}{old}";
            this.TextBlockDebugLog.Text = debugInfos;
        }

        private void btnCopyDebugToClipboard_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var content = this.TextBlockDebugLog.Text;
            if (string.IsNullOrEmpty(content))
            {
                System.Windows.MessageBox.Show("No content to copy");
                return;
            }
            System.Windows.Clipboard.SetText(content);
        }
    }
}

// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Services;
using Awb.Core.Tools;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AwbStudio.AwbClientsControls
{
    /// <summary>
    /// Lists all available AWB clients and allows to rescan for new clients.
    /// </summary>
    public partial class AwbClientsControl : UserControl
    {
        private readonly IAwbClientsService _awbClientsService;
        private readonly IInvokerService _invokerService;

        public AwbClientsControl()
        {
            _awbClientsService = App.GetService<IAwbClientsService>();
            _invokerService = App.GetService<IInvokerService>();
            InitializeComponent();
            Loaded += AwbClientsControl_Loaded;
        }

        private async void AwbClientsControl_Loaded(object sender, RoutedEventArgs e)
        {
            _awbClientsService.ClientsLoaded += AwbClientsService_ClientsLoaded;
            if (_awbClientsService.ComPortClients != null) await ShowClients();
        }

        private async void AwbClientsService_ClientsLoaded(object? sender, EventArgs e)
        {
            await ShowClients();
            tabsClients.SelectionChanged += (s, args) =>
            {
                // when the selected tab changes, we need to update the client control
                if (tabsClients.SelectedItem is TabItem selectedTab)
                {
                    foreach (AwbClientControl clientControl in gridClients.Children)
                    {
                        var isSelected = clientControl == selectedTab.Tag as AwbClientControl;
                        clientControl.Visibility = isSelected ? Visibility.Visible : Visibility.Collapsed;
                    }
                }
            };
        }


        private async Task ShowClients()
        {
            // wrap into an new Task to avoid cross-thread exception
            await Task.Run(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    var clients = _awbClientsService.ComPortClients;
                    this.gridClients.Children.Clear();
                    this.tabsClients.Items.Clear();
                    foreach (var client in clients)
                    {
                        // add the client
                        var clientControl = new AwbClientControl(_invokerService);
                        this.gridClients.Children.Add(clientControl);
                        clientControl.SetClient(client);

                        // create a new tab for each client
                        var tabItem = new TabItem
                        {
                            Header = client.FriendlyName,
                            Content = new TextBlock { Text = $"Client '{client.FriendlyName}'" },
                            Tag = clientControl
                        };
                        this.tabsClients.Items.Add(tabItem);
                    }
                    if (tabsClients.Items.Count > 0)
                        this.tabsClients.SelectedIndex = 0; // select the first tab by default

                    labelClientCount.Content = $"{clients.Length} clients found";
                });
            });
        }

        private async void ButtonRescan_Click(object sender, RoutedEventArgs e)
        {
            var result = await _awbClientsService.ScanForClients(false);
        }
    }
}

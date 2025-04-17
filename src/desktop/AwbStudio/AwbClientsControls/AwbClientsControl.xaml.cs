// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Services;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AwbStudio.AwbClientsControls
{
    /// <summary>
    /// Interaction logic for AwbClientsControl.xaml
    /// </summary>
    public partial class AwbClientsControl : UserControl
    {
        private readonly IAwbClientsService _awbClientsService;

        public AwbClientsControl()
        {
            _awbClientsService = App.GetService<IAwbClientsService>();
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
        }

        private async Task ShowClients()
        {
            // wrap into an new Task to avoid cross-thread exception
            await Task.Run(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    var clients = _awbClientsService.ComPortClients;
                    this.stackPanelClients.Children.Clear();
                    foreach (var client in clients)
                    {
                        var clientControl = new AwbClientControl();
                        this.stackPanelClients.Children.Add(clientControl);
                        clientControl.SetClient(client);
                    }

                    labelClientCount.Content = $"{clients.Length} clients found";
                });
            });
        }

        private async void ButtonRescan_Click(object sender, RoutedEventArgs e)
        {
            var result = await _awbClientsService.ScanForClients(false);
            if (result > 0)
            {
                labelClientCount.Content = $"{result} clients found";
            }
            else
            {
                labelClientCount.Content = "No clients found";
            }
            await ShowClients();
        }
    }
}

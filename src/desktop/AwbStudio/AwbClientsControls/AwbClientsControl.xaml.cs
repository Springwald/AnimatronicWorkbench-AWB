// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Services;
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
    /// Interaction logic for AwbClientsControl.xaml
    /// </summary>
    public partial class AwbClientsControl : UserControl
    {
        private readonly IAwbClientsService _awbClientsService;

        public AwbClientsControl()
        {
            _awbClientsService =  App.GetService<IAwbClientsService>();
            InitializeComponent();
            Loaded += AwbClientsControl_Loaded;
        }

        private async void AwbClientsControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_awbClientsService.ComPortClients == null)
            {
                _awbClientsService.ClientsLoaded += AwbClientsService_ClientsLoaded;
            }
            else
            {
                await ShowClients();
            }
        }

        private async void AwbClientsService_ClientsLoaded(object? sender, EventArgs e)
        {
           await  ShowClients();
        }

        private async Task ShowClients()
        {
            var clients = _awbClientsService.ComPortClients;
            this.stackPanelClients.Children.Clear();
            foreach (var client in clients)
            {
                var clientControl = new AwbClientControl();
                this.stackPanelClients.Children.Add(clientControl);
            }
            labelClientCount.Content = $"{clients.Length} clients found";
            await Task.CompletedTask;
        }
    }
}

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

        public AwbClientsControl(IAwbClientsService awbClientsService)
        {
            _awbClientsService = awbClientsService;
            InitializeComponent();
            Loaded += AwbClientsControl_Loaded;

        }

        private async void AwbClientsControl_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadClients();
        }

        private async Task LoadClients()
        {
            var clients = 
        }
    }
}

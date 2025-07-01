// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using System.Diagnostics;
using System.Windows;

namespace AwbStudio
{

    /// <summary>
    /// Interaction logic for AwbClientsWindow.xaml
    /// </summary>
    public partial class AwbClientsWindow : Window
    {
        public AwbClientsWindow()
        {
            InitializeComponent();
            Loaded += AwbClientsWindow_Loaded;
        }

        private void AwbClientsWindow_Loaded(object sender, RoutedEventArgs e)
        {
           if (!Debugger.IsAttached) 
                this.WindowState = WindowState.Minimized;
        }
    }
}

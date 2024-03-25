// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace AwbStudio
{
    /// <summary>
    /// Interaction logic for DebugWindow.xaml
    /// </summary>
    public partial class DebugWindow : Window
    {
        private DispatcherTimer timer;

        public TextBox? TextBox => this.TextBoxDebugOutput;

        public DebugWindow()
        {
            InitializeComponent();
            TextBoxDebugOutput.Text = string.Empty;
            ShowInTaskbar = true;
            WindowState = WindowState.Minimized;
            Loaded += DebugWindow_Loaded;
        }

        private void DebugWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (false && Debugger.IsAttached)
            {
                WindowState = WindowState.Normal;
                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(1);
                timer.Tick += Timer_Tick;
                timer.Start();
            } 
            
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            this.BringIntoView();
            this.Topmost = true;
        }
    }
}

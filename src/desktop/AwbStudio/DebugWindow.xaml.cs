// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace AwbStudio
{
    /// <summary>
    /// Interaction logic for DebugWindow.xaml
    /// </summary>
    public partial class DebugWindow : Window
    {
        public TextBox? TextBox => this.TextBoxDebugOutput;

        public DebugWindow()
        {
            InitializeComponent();
            TextBoxDebugOutput.Text = "";
            ShowInTaskbar = true;
            WindowState = Debugger.IsAttached ? WindowState.Normal :  WindowState.Minimized;
        }
    }
}

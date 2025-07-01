// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using System.Windows;
using System.Windows.Controls;

namespace AwbStudio
{
    public partial class DebugWindow : Window
    {
        public TextBox? TextBox => this.TextBoxDebugOutput;

        public DebugWindow()
        {
            InitializeComponent();
            TextBoxDebugOutput.Text = string.Empty;
            ShowInTaskbar = true;
            WindowState = WindowState.Minimized;
        }
    }
}

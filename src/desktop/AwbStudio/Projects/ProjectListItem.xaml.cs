// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using System;
using System.Windows.Controls;

namespace AwbStudio.Projects
{
    public partial class ProjectListItem : UserControl
    {
        public EventHandler? OnOpenProjectClicked;
        public EventHandler? OnRemoveProjectClicked;
        public EventHandler? OnConfigProjectClicked;

        public string Title
        {
            set { buttonLoadProject.Content = value; }
        }

        public ProjectListItem()
        {
            InitializeComponent();
        }

        private void buttonProject_Click(object sender, System.Windows.RoutedEventArgs e)
        => this.OnOpenProjectClicked?.Invoke(this, EventArgs.Empty);

        private void buttonEditConfig_Click(object sender, System.Windows.RoutedEventArgs e)
        => this.OnConfigProjectClicked?.Invoke(this, EventArgs.Empty);

        private void buttonRemoveProject_Click(object sender, System.Windows.RoutedEventArgs e)
        => this.OnRemoveProjectClicked?.Invoke(this, EventArgs.Empty);
    }
}

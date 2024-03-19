// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using AwbStudio.Projects;
using System.Windows;

namespace AwbStudio
{
    /// <summary>
    /// Interaction logic for ProjectConfigurationWindow.xaml
    /// </summary>
    public partial class ProjectConfigurationWindow : Window
    {
        public ProjectConfigurationWindow(IProjectManagerService projectManagerService)
        {
            InitializeComponent();
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            //todo: ask if unsaved changes should be saved

            this.Close();
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            //todo: save the project configuration

            this.Close();
        }
    }
}

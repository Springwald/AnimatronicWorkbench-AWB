// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Export;
using AwbStudio.Projects;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace AwbStudio.Exports
{
    public partial class ExportToClientsWindow : Window
    {
        private readonly Brush _rememberBackground;
        private readonly IProjectManagerService _projectManagerService;

        protected string? ExportFolder
        {
            get => LabelTargetFolder?.Content?.ToString();
            set
            {
                LabelTargetFolder.Content = value;
                LabelTargetFolder.Background = Directory.Exists(value) ? null : new SolidColorBrush(Colors.LightSalmon);
            }
        }

        public ExportToClientsWindow(IProjectManagerService projectManagerService)
        {
            InitializeComponent();
            _rememberBackground = this.Background;

            _projectManagerService = projectManagerService;
            if (projectManagerService.ActualProject == null)
            {
                MessageBox.Show("No project loaded");
                return;
            }

            ExportFolder = System.IO.Path.Combine(projectManagerService.ActualProject.ProjectFolder, "Esp32Clients");

            Loaded += ExportToClientCodeWindow_Loaded;
        }

        private void ExportToClientCodeWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= ExportToClientCodeWindow_Loaded;
        }

        //private async void LabelTargetFolder_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        //{
        //    if (_projectManagerService.ActualProject == null)
        //    {
        //        MessageBox.Show("No project loaded");
        //        return;
        //    }

        //    using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
        //    {
        //        System.Windows.Forms.DialogResult result = dialog.ShowDialog();
        //        if (result == System.Windows.Forms.DialogResult.OK)
        //        {
        //            var folder = dialog.SelectedPath;
        //            _projectManagerService.ActualProject.Esp32ExportFolder = folder;
        //            var saved = await _projectManagerService.SaveProjectAsync(_projectManagerService.ActualProject, _projectManagerService.ActualProject.ProjectFolder);
        //            if (!saved)
        //            {
        //                MessageBox.Show("Error saving project");
        //                return;
        //            }
        //            ExportFolder = folder;
        //        }
        //    }
        //}


        private void ButtonWriteToEsp32RemoteClient_Click(object sender, RoutedEventArgs e)
        {
            var sourceFolder = "";
            var exporter = new Esp32ClientRemoteExporter(sourceFolder);
        }
    }
}

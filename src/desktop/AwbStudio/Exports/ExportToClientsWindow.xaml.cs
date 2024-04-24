// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Export;
using Awb.Core.Services;
using AwbStudio.Projects;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace AwbStudio.Exports
{
    public partial class ExportToClientsWindow : Window
    {
        private readonly IProjectManagerService _projectManagerService;
        private readonly Brush _rememberBackground;
        private readonly IAwbLogger _awbLogger;

        protected string? ExportFolderGlobal
        {
            get => LabelTargetFolder?.Content?.ToString();
            set
            {
                LabelTargetFolder.Content = value;
                LabelTargetFolder.Background = Directory.Exists(value) ? null : new SolidColorBrush(Colors.LightSalmon);
            }
        }

        public ExportToClientsWindow(IProjectManagerService projectManagerService, IAwbLogger awbLogger)
        {
            InitializeComponent();
            _rememberBackground = this.Background;
            _awbLogger = awbLogger;

            _projectManagerService = projectManagerService;
            if (projectManagerService.ActualProject == null)
            {
                MessageBox.Show("No project loaded");
                return;
            }

            ExportFolderGlobal = System.IO.Path.Combine(projectManagerService.ActualProject.ProjectFolder, "Esp32Clients");

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


        private async void ButtonWriteToEsp32RemoteClient_Click(object sender, RoutedEventArgs e)
        {
            // get the folder where the wpf project is located and running:
            var appFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            var esp32ClientsSourceFolderRelative = @"..\..\..\..\..\clients";

            var esp32ClientsSourceFolder =  Path.Combine(appFolder, esp32ClientsSourceFolderRelative);

            await Export(new Esp32ClientRemoteExporter(esp32ClientsSourceFolder: esp32ClientsSourceFolder), targetSubFolder: "Esp32RemoteClient");   
        }

        private async Task Export(IExporter exporter,  string targetSubFolder)
        {

            var targetFolder = Path.Combine(ExportFolderGlobal, targetSubFolder);
            if (!Directory.Exists(targetFolder))
            {
                var msgBoxResult = MessageBox.Show("Target folder \r\n\r\n'" + targetFolder + "'\r\n\r\n does not exist.\r\n\r\nCreate?", "Create target folder?", MessageBoxButton.YesNoCancel);
                if (msgBoxResult == MessageBoxResult.Yes)
                {
                    Directory.CreateDirectory(targetFolder);
                }
                else
                {
                    MessageBox.Show("Export canceled");
                    return;
                }
            }

            await _awbLogger.LogAsync("Exporting to " + targetFolder);
           
            exporter.Processing += ExporerProcessing;

            var result = await exporter.ExportAsync(targetPath: targetFolder);

            exporter.Processing -= ExporerProcessing;

            if (result.Success)
            {
                MessageBox.Show("Export done.");
            }
            else
            {
                MessageBox.Show("Export failed: " + result.ErrorMessage);
            }
        }

        private async void ExporerProcessing(object? sender, ExporterProcessStateEventArgs e)
        {
            if (e.ErrorMessage == null)
            {
                // todo: show progress
            }
            else
            {
                await _awbLogger.LogErrorAsync(e.ErrorMessage);
            }
        }
    }
}

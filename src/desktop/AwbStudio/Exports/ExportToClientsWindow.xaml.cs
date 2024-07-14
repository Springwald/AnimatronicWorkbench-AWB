// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Export;
using Awb.Core.Export.ExporterParts;
using Awb.Core.Project;
using Awb.Core.Services;
using AwbStudio.Projects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace AwbStudio.Exports
{
    public partial class ExportToClientsWindow : Window
    {
        private readonly IProjectManagerService _projectManagerService;
        private readonly Brush _rememberBackground;
        private readonly IAwbLogger _awbLogger;
        private readonly AwbProject _project;

        private WifiConfigExportData WifiConfigData => new WifiConfigExportData { 
            WlanSSID = _project.ProjectMetaData.WifiSsid, 
            WlanPassword = _project.ProjectMetaData.WifiPassword 
        };

        private ProjectExportData? ProjectData
        {
            get
            {
                var timelines = new List<TimelineExportData>();
                var timelineIds = _project.TimelineDataService.TimelineIds;

                foreach (var timelineId in timelineIds)
                {
                    var timelineData = _project.TimelineDataService.GetTimelineData(timelineId);
                    if (timelineData == null)
                    {
                        MessageBox.Show($"Can't load timeline '{timelineId}'. Export canceled.");
                        return null;
                    }

                    // convert timelineData to TimelineExportData
                    var exportData = TimelineExportData.FromTimeline(
                        timelineStateId: timelineData.TimelineStateId,
                        title: timelineData.Title,
                        points: timelineData.AllPoints,
                        timelineDataService: _project.TimelineDataService,
                        awbLogger: _awbLogger);

                    timelines.Add(exportData);
                }

                return new ProjectExportData
                {
                    ProjectName = _project.ProjectMetaData.ProjectTitle,
                    TimelineStates = _project.TimelinesStates,
                    StsServoConfigs = _project.StsServos,
                    ScsServoConfigs = _project.ScsServos,
                    Pca9685PwmServoConfigs = _project.Pca9685PwmServos,
                    Mp3PlayerYX5300Configs = _project.Mp3PlayersYX5300,
                    Esp32ClientHardwareConfig = _project.Esp32ClientHardware,
                    InputConfigs = _project.Inputs,
                    TimelineData = timelines.ToArray()
                };
            }
        }

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
            _project = projectManagerService.ActualProject;

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
            labelOutput.Content = string.Empty;

            // get the folder where the wpf project is located and running:
            var appFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            if (appFolder == null)
            {
                await LogAsync("Could not determine the application folder", error: true);
                return;
            }

            var esp32ClientsSourceFolderRelative = @"..\..\..\..\..\clients";
            var esp32ClientsSourceFolder = Path.Combine(appFolder, esp32ClientsSourceFolderRelative);

            var projectData = ProjectData;
            if (projectData == null)
            {
                await LogAsync("Project data could not be loaded", error: true);
                return;
            }

            await Export(new Esp32ClientExporter(esp32ClientsSourceFolder: esp32ClientsSourceFolder, WifiConfigData, projectData), targetSubFolder: "awb_esp32_client");
            await Export(new Esp32ClientRemoteM5StickJoyCExporter(esp32ClientsSourceFolder: esp32ClientsSourceFolder, WifiConfigData), targetSubFolder: "awb_esp32_remote-M5Stick-Mini-JoyC-HAT");
        }



        private async Task Export(IExporter exporter, string targetSubFolder)
        {
            await LogAsync("-------------------------------");

            await LogAsync($"Exporting {exporter.Title}");

            if (string.IsNullOrWhiteSpace(ExportFolderGlobal))
            {
                await LogAsync("No ExportFolderGlobal set!", error: true);
                return;
            }

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
                    await LogAsync("Export canceled");
                    return;
                }
            }

            await LogAsync($"Exporting to '{targetFolder}'");

            exporter.Processing += ExporterProcessing;

            var result = await exporter.ExportAsync(targetPath: targetFolder);

            exporter.Processing -= ExporterProcessing;

            if (result.Success)
            {
                await LogAsync("Export done.");
            }
            else
            {
                await LogAsync($"Export failed: {result.ErrorMessage ?? "Unknown error"}", error: true);
            }
        }

        private async void ExporterProcessing(object? sender, ExporterProcessStateEventArgs e)
        {
            if (e.ErrorMessage != null)
            {
                await LogAsync(e.ErrorMessage, error: true);
            }

            if (e.Message != null)
            {
                await LogAsync(e.Message);
            }
        }

        private async Task LogAsync(string message, bool error = false)
        {
            if (error)
            {
                labelOutput.Content += "\r\nERROR: " + message;
                await _awbLogger.LogErrorAsync(message);
            }
            else
            {
                labelOutput.Content += "\r\n" + message;
                await _awbLogger.LogAsync(message);
            }
            await Task.CompletedTask;
        }
    }
}

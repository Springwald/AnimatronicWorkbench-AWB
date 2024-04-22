// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.LoadNSave.Export;
using AwbStudio.Projects;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace AwbStudio.Exports
{
    public partial class ExportToClientCodeWindow : Window
    {
        const string targetFilenameOnly = "AutoPlayData.h";

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

        public ExportToClientCodeWindow(IProjectManagerService projectManagerService)
        {
            InitializeComponent();
            _rememberBackground = this.Background;

            _projectManagerService = projectManagerService;
            if (projectManagerService.ActualProject == null)
            {
                MessageBox.Show("No project loaded");
                return;
            }   

            ExportFolder = projectManagerService.ActualProject.AutoPlayEsp32ExportFolder;

            ButtonCopyToFile.Content = $"Write to '{targetFilenameOnly}' file";

            Loaded += ExportToClientCodeWindow_Loaded;
        }

        private void ExportToClientCodeWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= ExportToClientCodeWindow_Loaded;
        }

        private async void LabelTargetFolder_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_projectManagerService.ActualProject == null)
            {
                MessageBox.Show("No project loaded");
                return;
            }

            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    var folder = dialog.SelectedPath;
                    var targetFilename = Path.Combine(folder, targetFilenameOnly);
                    if (!File.Exists(targetFilename))
                    {
                        MessageBox.Show($"No '{targetFilenameOnly}' found in folder '{ExportFolder}'");
                        return;
                    }
                    _projectManagerService.ActualProject.AutoPlayEsp32ExportFolder = folder;
                    var saved = await _projectManagerService.SaveProjectAsync(_projectManagerService.ActualProject, _projectManagerService.ActualProject.ProjectFolder);
                    if (!saved)
                    {
                        MessageBox.Show("Error saving project");
                        return;
                    }   
                    ExportFolder = folder;
                }
            }
        }

        private void ButtonCopyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(TextBlockExportCode.Text);
        }

        internal void ShowResult(Esp32ExportResult result)
        {
            if (result.Ok)
            {
                this.Background = _rememberBackground;
                this.TextBlockExportCode.Text = result.Code;
                ButtonCopyToClipboard.IsEnabled = true;
                ButtonCopyToFile.IsEnabled = true;
            }
            else
            {
                this.Background = new SolidColorBrush(Colors.LightSalmon);
                this.TextBlockExportCode.Text = result.Message;
                ButtonCopyToClipboard.IsEnabled = false;
                ButtonCopyToFile.IsEnabled = false;
                MessageBox.Show(result.Message);
            }
        }

        private void ButtonCopyToFile_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(ExportFolder))
            {
                MessageBox.Show("Please select a valid target folder first.");
                return;
            }

            var targetFilename = Path.Combine(ExportFolder, targetFilenameOnly);
            if (!File.Exists(targetFilename))
            {
                MessageBox.Show($"No '{targetFilenameOnly}' found in folder '{ExportFolder}'");
                return;
            }

            try
            {
                File.WriteAllText(targetFilename, TextBlockExportCode.Text);
                MessageBox.Show($"Written to '{targetFilename}'");
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}

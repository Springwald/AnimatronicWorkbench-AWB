// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Configs;
using AwbStudio.Projects;
using AwbStudio.StudioSettings;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AwbStudio
{
    /// <summary>
    /// Interaction logic for ProjectManagementWindow.xaml
    /// </summary>
    public partial class ProjectManagementWindow : Window
    {
        private readonly IProjectManagerService _projectManagerService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IAwbStudioSettingsService _awbStudioSettingsService;

        public ProjectManagementWindow(IProjectManagerService projectManagerService, IAwbStudioSettingsService awbStudioSettingsService, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _projectManagerService = projectManagerService;
            _serviceProvider = serviceProvider;
            _awbStudioSettingsService = awbStudioSettingsService;
            ShowLatestProjects();
            this.KeyDown += OnKeyDown;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (this.IsVisible)
            {
                if (e.Key >= Key.D1 && e.Key <= Key.D9)
                {
                    var index = (int)(e.Key - Key.D1);
                    if (index < ListLatestProjects.Items.Count)
                    {
                        var projectFolder = (ListLatestProjects.Items[index] as ListBoxItem)?.ToolTip.ToString();
                        if (!string.IsNullOrWhiteSpace(projectFolder))
                        {
                            var ok = OpenProject(projectFolder);
                        }
                    }
                }
            }
        }

        private void ShowLatestProjects()
        {
            ListLatestProjects.Items.Clear();
            int no = 1;
            foreach (var projectFolder in _awbStudioSettingsService.StudioSettings.LatestProjectsFolders)
            {
                ListLatestProjects.Items.Add(new ListBoxItem()
                {
                    Content = $"[{no}] {projectFolder.Split('\\').LastOrDefault()}",
                    ToolTip = projectFolder,
                    Tag = no,
                });
                no++;
            }
        }

        private void ButtonCreateNew_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    var projectPath = dialog.SelectedPath;
                    if (_projectManagerService.ExistProject(projectPath))
                    {
                        System.Windows.MessageBox.Show($"Folder '{projectPath}' already contains an Animatronic Workbench project.");
                        return;
                    }

                    var project = new AwbProject(projectFolder: projectPath, title: "no project title")
                    {
                        Info = "Description of this AWB project",
                        TimelinesStates = new TimelineState[]
                        {
                            new TimelineState(1, "sleep"),
                            new TimelineState(2, "action"),
                            new TimelineState(3, "idle"),
                            new TimelineState(4, "talk"),
                            new TimelineState(5, "joint demo"),
                        },
                        StsServos = new StsServoConfig[]
                        {
                             new StsServoConfig("StsServo1", clientId: 1, channel: 1)
                             {
                                 Name ="Demo serial Servo 1",
                                 Accelleration = 20,
                                 DefaultValue = 2000,
                                 MaxValue = 4095,
                                 MinValue = 0,
                                 Speed = 1000,
                             },
                         }
                    };

                    if (_projectManagerService.SaveProject(project, projectFolder: dialog.SelectedPath))
                    {
                        if (_projectManagerService.OpenProject(projectPath, out string[] errorMessages))
                        {
                            // ok, project created and opened
                            ShowProjectEditor();
                        }
                        else
                        {
                            foreach (var error in errorMessages)
                                MessageBox.Show(error, "Error opening project");
                        }
                    }
                }
            }
        }

        private void ButtonOpenExisting_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    var folder = dialog.SelectedPath;
                    var ok = OpenProject(folder);
                }
            }
        }

        private void ListLatestProjects_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var folder = (ListLatestProjects.SelectedItem as ListBoxItem)?.ToolTip.ToString();
            if (folder != null)
            {
                var ok = OpenProject(folder);
            }
        }

        private bool OpenProject(string projectPath)
        {
            if (!_projectManagerService.ExistProject(projectPath))
            {
                System.Windows.MessageBox.Show($"No Animatronic Workbench project found in folder '{projectPath}'");
                return false;
            }
            if (_projectManagerService.OpenProject(projectPath, out string[] errorMessages))
            {
                //_projectManagerService.SaveProject(_projectManagerService.ActualProject, projectPath);
                // ok, project opened
                ShowProjectEditor();
                return true;
            }
            else
            {
                foreach (var error in errorMessages)
                {
                    MessageBox.Show(error, "Error opening project");
                }
                return false;
            }
        }

        private void ShowProjectEditor()
        {
            var project = _projectManagerService.ActualProject;
            if (project != null)
            {
                var timelineEditorWindow = _serviceProvider.GetService<TimelineEditorWindow>();
                if (timelineEditorWindow != null)
                {
                    this.Hide();
                    timelineEditorWindow.Show();
                    timelineEditorWindow.Closed += (s, args) =>
                    {
                        this.Show();
                        ShowLatestProjects();
                    };
                }
            }
        }


    }
}

// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.InputControllers.TimelineInputControllers;
using Awb.Core.Project;
using Awb.Core.Services;
using AwbStudio.Projects;
using AwbStudio.StudioSettings;
using AwbStudio.Tools;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
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
        private const bool editConfigAvailable = false; // planned for future release

        private readonly IProjectManagerService _projectManagerService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IAwbStudioSettingsService _awbStudioSettingsService;
        private readonly IAwbLogger _awbLogger;

        public ProjectManagementWindow(IProjectManagerService projectManagerService, IAwbStudioSettingsService awbStudioSettingsService, IServiceProvider serviceProvider, IAwbLogger awbLogger)
        {
            InitializeComponent();
            _projectManagerService = projectManagerService;
            _serviceProvider = serviceProvider;
            _awbStudioSettingsService = awbStudioSettingsService;
            _awbLogger = awbLogger;

            this.KeyDown += OnKeyDown;

            _awbLogger.OnLog += (s, args) =>
            {
                MyInvoker.Invoke(new Action(() =>
                {
                    var msg = args;
                    if (msg.Length > 100) msg = msg.Substring(0, 100) + "...";
                    TextBoxDebugOutput.Text += msg + "\r\n";
                }));
            };
            _awbLogger.OnError += (s, args) =>
            {
                MyInvoker.Invoke(new Action(() =>
                {
                    var msg = args;
                    if (msg.Length > 100) msg = msg.Substring(0, 100) + "...";
                    TextBoxDebugOutput.Text += msg + "\r\n";
                }));
            };

            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            ShowLatestProjects();

            if (_awbStudioSettingsService.StudioSettings.ReOpenLastProjectOnStart)
            {
                ReOpenLastProjectCheckbox.IsChecked = true;
                var lastProjecFolder = _awbStudioSettingsService.StudioSettings.LatestProjectsFolders.FirstOrDefault();
                if (lastProjecFolder != null)
                {
                    var ok = await OpenProject(lastProjecFolder, editConfig: false);
                }
            }

            BringIntoView();
            if (editConfigAvailable == false)
            {
                ButtonEditExisting.Visibility = Visibility.Collapsed;
            }
        }

        private async void OnKeyDown(object sender, KeyEventArgs e)
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
                            var ok = await OpenProject(projectFolder, editConfig: false);
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

        private async void ButtonCreateNew_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    var projectPath = dialog.SelectedPath;
                    if (_projectManagerService.ExistProject(projectPath))
                    {
                        System.Windows.MessageBox.Show($"Folder '{projectPath}' already contains an Animatronic WorkBench project.");
                        return;
                    }

                    AwbProject project = CreateNewProject(projectPath);

                    if (await _projectManagerService.SaveProjectAsync(project, projectFolder: dialog.SelectedPath))
                    {
                        var openResult = await _projectManagerService.OpenProjectAsync(projectPath);
                        if (openResult.Success)
                        {
                            // ok, project created and opened
                            ShowProjectConfigEditor();
                        }
                        else
                        {
                            foreach (var error in openResult.ErrorMessages)
                                MessageBox.Show(error, "Error opening project");
                        }
                    }
                }
            }
        }

        private static AwbProject CreateNewProject(string projectPath)
        {
            var project = new AwbProject(title: "no project title")
            {
                Info = "Animatronic Workbench Project | https://daniel.springwald.de/post/AWB/AnimatronicWorkbench",
                TimelinesStates = new TimelineState[]
                {
                    new TimelineState(1, "sleep", export: true, positiveInputs: new[] { 1 }),
                    new TimelineState(2, "action", export: true),
                    new TimelineState(3, "idle", export: true),
                    new TimelineState(4, "talk", export: true),
                    new TimelineState(5, "joint demo", export: true),
                    new TimelineState(6, "tests", export: false),
                },
                StsServos = new StsServoConfig[]
                {
                    new StsServoConfig(id: "StsServo1", title: "Demo serial Servo 1", clientId: 1, channel: 1)
                    {
                        Acceleration = 20,
                        DefaultValue = 2000,
                        MaxValue = 4095,
                        MinValue = 0,
                        Speed = 1000,
                    },
                },
                Pca9685PwmServos = new Pca9685PwmServoConfig[]
                {
                    new Pca9685PwmServoConfig(id: "PwmServo1",  title:"Demo PWM Servo 1" , clientId: 1, i2cAdress: 0x40, channel: 1)
                    {
                        DefaultValue = 2000,
                        MaxValue = 4095,
                        MinValue = 0,
                    },
                },
                Mp3PlayersYX5300 = new[] { new Mp3PlayerYX5300Config(clientId: 1, rxPin: 13, txPin: 14, soundPlayerId: "YX5300_1", name: "Mp3Player") },
                Inputs = new InputConfig[]
                {
                    new InputConfig(id: 1, name:"Sleep") { IoPin = 25 }
                },

            };
            project.SetProjectFolder(projectPath);
            return project;
        }

        private async void ButtonOpenExisting_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    var folder = dialog.SelectedPath;
                    var ok = await OpenProject(folder, editConfig: false);
                }
            }
        }

        private async void ButtonEditExisting_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    var folder = dialog.SelectedPath;
                    var ok = await OpenProject(folder, editConfig: true);
                }
            }
        }

        private async void ListLatestProjects_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var folder = (ListLatestProjects.SelectedItem as ListBoxItem)?.ToolTip.ToString();
            if (folder != null)
            {
                var ok = await OpenProject(folder, editConfig: false);
            }
        }

        private async Task<bool> OpenProject(string projectPath, bool editConfig)
        {
            if (!_projectManagerService.ExistProject(projectPath))
            {
                MessageBox.Show($"No Animatronic WorkBench project found in folder '{projectPath}'");
                return false;
            }
            var openResult = await _projectManagerService.OpenProjectAsync(projectPath);
            if (openResult.Success)
            {
                // ok, project opened
                if (editConfigAvailable == true && editConfig == true)
                    ShowProjectConfigEditor();
                else
                    await LoadProject();
                return true;
            }
            else
            {
                foreach (var error in openResult.ErrorMessages)
                {
                    MessageBox.Show(error, "Error opening project");
                }
                return false;
            }
        }

        private void ShowProjectConfigEditor()
        {
            var project = _projectManagerService.ActualProject;
            if (project != null)
            {
                var configurationEditorWindow = _serviceProvider.GetService<ProjectConfigurationWindow>();
                if (configurationEditorWindow != null)
                {
                    this.Hide();
                    configurationEditorWindow.Show();
                    configurationEditorWindow.Closed += (s, args) =>
                    {
                        this.Show();
                        ShowLatestProjects();
                    };
                }
            }
        }

        private async Task LoadProject()
        {
            var project = _projectManagerService.ActualProject;
            if (project != null)
            {
                // show the loading screen
                TextBoxDebugOutput.Text = $"loading project\r\n'{project.ProjectFolder}'...\r\n\r\n";
                this.ShowLoadingScreen(true);

                var inputControllerService = _serviceProvider.GetService<IInputControllerService>();
                if (inputControllerService == null)
                {
                    MessageBox.Show("No input controller service available");
                    this.ShowLoadingScreen(false);
                    return;
                }
                var timelineControllers = inputControllerService.TimelineControllers;

                var clientsService = _serviceProvider.GetService<IAwbClientsService>();
                if (clientsService == null)
                {
                    MessageBox.Show("No clients service available");
                    this.ShowLoadingScreen(false);
                    return;
                }
                clientsService.ClientsLoaded += (s, args) =>
                {
                    ShowProjectTimelineEditor(clientsService, timelineControllers);
                };
                await clientsService.InitAsync();
            }
        }

        private void ShowProjectTimelineEditor(IAwbClientsService clientsService, ITimelineController[] timelineControllers)
        {
            var timelineEditorWindow = new TimelineEditorWindow(timelineControllers, _projectManagerService, clientsService, _awbLogger);
            if (timelineEditorWindow != null)
            {
                // hide the loading screen
                ShowLoadingScreen(false);
                this.Hide();
                timelineEditorWindow.Show();
                timelineEditorWindow.Closed += (s, args) =>
                {
                    this.Show();
                    ShowLatestProjects();
                };
            }
        }

        private void ShowLoadingScreen(bool show)
        {
            GridProjectManagement.Visibility = show ? Visibility.Collapsed : Visibility.Visible;
            GridLoadingProject.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void ReOpenLastProjectCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            _awbStudioSettingsService.StudioSettings.ReOpenLastProjectOnStart = ReOpenLastProjectCheckbox.IsChecked == true;
            await _awbStudioSettingsService.SaveSettingsAsync();
        }
    }
}

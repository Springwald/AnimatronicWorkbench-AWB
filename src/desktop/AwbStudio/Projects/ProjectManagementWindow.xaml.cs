// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.InputControllers.TimelineInputControllers;
using Awb.Core.Project;
using Awb.Core.Project.Various;
using Awb.Core.Services;
using Awb.Core.Tools;
using AwbStudio.Projects;
using AwbStudio.StudioSettings;
using AwbStudio.Tools;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AwbStudio
{
    public partial class ProjectManagementWindow : Window
    {
        private readonly IProjectManagerService _projectManagerService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IAwbStudioSettingsService _awbStudioSettingsService;
        private readonly IInvokerService _invokerService;
        private readonly IAwbLogger _awbLogger;

        public ProjectManagementWindow(
            IProjectManagerService projectManagerService,
            IAwbStudioSettingsService awbStudioSettingsService,
            IServiceProvider serviceProvider,
            IInvokerService invokerService,
            IAwbLogger awbLogger)
        {
            InitializeComponent();
            _projectManagerService = projectManagerService;
            _serviceProvider = serviceProvider;
            _awbStudioSettingsService = awbStudioSettingsService;
            _invokerService = invokerService;
            _awbLogger = awbLogger;

            this.KeyDown += OnKeyDown;

            // show global errs and debug output
            _awbLogger.OnLog += (s, args) =>
            {
                WpfAppInvoker.Invoke(new Action(() =>
                {
                    var msg = args;
                    if (msg.Length > 100) msg = msg.Substring(0, 100) + "...";
                    TextBoxDebugOutput.Text += msg + "\r\n";
                }), System.Windows.Threading.DispatcherPriority.Background);
            };
            _awbLogger.OnError += (s, args) =>
            {
                WpfAppInvoker.Invoke(new Action(() =>
                {
                    var msg = args;
                    if (msg.Length > 100) msg = msg.Substring(0, 100) + "...";
                    TextBoxDebugOutput.Text += msg + "\r\n";
                }), System.Windows.Threading.DispatcherPriority.Background);
            };

            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            ShowLatestProjects();
            BringIntoView();

            if (_awbStudioSettingsService.StudioSettings.ReOpenLastProjectOnStart)
            {
                ReOpenLastProjectCheckbox.IsChecked = true;
                var lastProjecFolder = _awbStudioSettingsService.StudioSettings.LatestProjectsFolders.FirstOrDefault();
                if (lastProjecFolder != null)
                {
                    var ok = await OpenProjectAsync(lastProjecFolder, editConfig: false);
                }
            }
        }

        private async void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (this.IsVisible)
            {
                if (e.Key >= Key.D1 && e.Key <= Key.D9)
                {
                    var index = (int)(e.Key - Key.D1);
                    if (index < _awbStudioSettingsService.StudioSettings.LatestProjectsFolders.Length)
                    {
                        var projectFolder = _awbStudioSettingsService.StudioSettings.LatestProjectsFolders[index];
                        if (!string.IsNullOrWhiteSpace(projectFolder))
                        {
                            var ok = await OpenProjectAsync(projectFolder, editConfig: false);
                        }
                    }
                }
            }
        }

        private void ShowLatestProjects()
        {
            StackPanelProjectList.Children.Clear();
            int no = 1;
            foreach (var projectFolder in _awbStudioSettingsService.StudioSettings.LatestProjectsFolders)
            {
                var title = $"[{no}] {System.IO.Path.GetFileName(projectFolder)}";
                var projectListItem = new ProjectListItem();
                StackPanelProjectList.Children.Add(projectListItem);
                projectListItem.Title = $"[{no}] {projectFolder.Split('\\').LastOrDefault()}";

                projectListItem.OnOpenProjectClicked += async (s, args) =>
                {
                    // open the project
                    var ok = await OpenProjectAsync(projectFolder, editConfig: false);
                };

                projectListItem.OnConfigProjectClicked += async (s, args) =>
                {
                    // edit the project configuration
                    var ok = await OpenProjectAsync(projectFolder, editConfig: true);
                };

                projectListItem.OnRemoveProjectClicked += async (s, args) =>
                {
                    if (MessageBox.Show($"Do you really want to remove the project '{projectFolder}'?", "Delete project", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        // remove the project folder from the list
                        var latestProjects = _awbStudioSettingsService.StudioSettings.LatestProjectsFolders;
                        latestProjects = latestProjects.Where(x => x != projectFolder).ToArray();
                        _awbStudioSettingsService.StudioSettings.LatestProjectsFolders = latestProjects;
                        await _awbStudioSettingsService.SaveSettingsAsync();
                        ShowLatestProjects();
                    }
                };
                no++;
            }
        }

        /// <summary>
        /// Create a new project in a folder chosen by the user.
        /// </summary>
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
                        MessageBox.Show($"Folder '{projectPath}' already contains an Animatronic WorkBench project.");
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

        /// <summary>
        /// Create a new project with default settings.
        /// </summary>
        private static AwbProject CreateNewProject(string projectPath)
        {
            var project = new AwbProject()
            {
                ProjectMetaData = new ProjectMetaData(),
                TimelinesStates = new TimelineState[] {
                    new TimelineState  { Id=1, Title ="Default", Export = true, AutoPlay = true },
                    new TimelineState  { Id=2, Title ="Dont export", Export = false, AutoPlay = false },
                },
                StsServos = [],
                ScsServos = [],
                Pca9685PwmServos = [],
                Mp3PlayersYX5300 = [],
                Mp3PlayersDFPlayerMini = [],
                Inputs = [],
            };
            project.SetProjectFolder(projectPath);
            return project;
        }

        private async void ButtonOpenExisting_Click(object sender, RoutedEventArgs e)
        {
            var folder = ChooseExistingProjectFolder();
            if (folder == null) return;
            var ok = await OpenProjectAsync(folder, editConfig: false);
        }

        private async void ButtonEditConfigurationExisting_Click(object sender, RoutedEventArgs e)
        {
            var folder = ChooseExistingProjectFolder();
            if (folder != null) await OpenProjectAsync(folder, editConfig: true);
        }


        private async Task<bool> OpenProjectAsync(string projectPath, bool editConfig)
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
                if (editConfig == true)
                    ShowProjectConfigEditor();
                else
                    await LoadProjectAsync();
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

        private async Task LoadProjectAsync()
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
                ShowProjectTimelineEditor(clientsService, timelineControllers);
            }
            await Task.CompletedTask;
        }

        private void ShowProjectTimelineEditor(IAwbClientsService clientsService, ITimelineController[] timelineControllers)
        {
            var actualProject = _projectManagerService.ActualProject;
            if (actualProject == null)
            {
                MessageBox.Show("No project loaded");
                ShowLoadingScreen(false);
                return;
            }

            var timelineEditorWindow = new TimelineEditorWindow(
                timelineControllers,
                _projectManagerService,
                clientsService,
                actualProject.TimelineDataService.TimelineMetaDataService,
                _invokerService,
                _awbLogger);

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

        private string? ChooseExistingProjectFolder()
        {
            const string extension = ".awbprj";

            // Create OpenFileDialog
            Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension  
            openFileDlg.DefaultExt = extension;
            openFileDlg.Filter = $"Animatronic WorkBench project ({extension})|*{extension}";

            // Launch OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = openFileDlg.ShowDialog();
            // Get the selected file name and display in a TextBox.
            // Load content of file in a TextBlock
            if (result == false) return null;

            var filename = openFileDlg.FileName;
            if (string.IsNullOrWhiteSpace(filename))
            {
                MessageBox.Show("No file selected");
                return null;
            }
            var folder = System.IO.Path.GetDirectoryName(filename);
            if (folder == null)
            {
                MessageBox.Show($"No folder found for file '{filename}'");
                return null;
            }
            return folder;
        }

    }
}

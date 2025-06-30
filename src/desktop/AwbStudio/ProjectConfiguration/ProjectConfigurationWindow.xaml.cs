// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Project;
using Awb.Core.Project.Servos;
using Awb.Core.Project.Various;
using Awb.Core.Services;
using Awb.Core.Timelines;
using Awb.Core.Tools;
using AwbStudio.ProjectConfiguration;
using AwbStudio.ProjectConfiguration.PropertyEditors;
using AwbStudio.Projects;
using AwbStudio.PropertyControls;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace AwbStudio
{
    /// <summary>
    /// Interaction logic for ProjectConfigurationWindow.xaml
    /// </summary>
    public partial class ProjectConfigurationWindow : Window
    {

        private ProjectConfigViewModel _viewModel;
        private readonly IAwbClientsService _awbClientsService;
        private readonly IInvokerService _invokerService;
        private readonly IProjectManagerService _projectManagerService;
        private readonly AwbProject _awbProject;
        private readonly IdCreator _idCreator;

        private TimelineData[]? _timelines;
        private TimelineData[] Timelines
        {
            get
            {
                if (_timelines == null)
                {
                    var ids = _awbProject.TimelineDataService.TimelineIds.ToArray();
                    var timelines = ids.Select(id => _awbProject.TimelineDataService.GetTimelineData(id)).ToArray() ?? Array.Empty<TimelineData>();
                    _timelines = timelines;
                }
                return _timelines;
            }
        }

        public ProjectConfigurationWindow(IProjectManagerService projectManagerService, IAwbClientsService awbClientsService, IInvokerService invokerService)
        {
            _awbClientsService = awbClientsService;
           _invokerService = invokerService;
            _projectManagerService = projectManagerService;
            _awbProject = projectManagerService.ActualProject ?? throw new ArgumentNullException(nameof(projectManagerService.ActualProject));
            _viewModel = new ProjectConfigViewModel(_awbProject);

            _idCreator = new IdCreator(_viewModel, _awbProject);

            this.DataContext = _viewModel;

            InitializeComponent();

            Loaded += ProjectConfigurationWindow_Loaded;
        }

        private async void ProjectConfigurationWindow_Loaded(object sender, RoutedEventArgs e)
        {
            PropertyEditor.AwbClientsService = _awbClientsService;

            // select the project properties as default
            SetObjectToEdit(_viewModel.ProjectMetaData);

            await ShowProjectProblems();

            Loaded -= ProjectConfigurationWindow_Loaded;
            Closing += ProjectConfigurationWindow_Closing;
            Unloaded += ProjectConfigurationWindow_Unloaded;
        }
        private void ProjectConfigurationWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            Closing += ProjectConfigurationWindow_Closing;
            Unloaded -= ProjectConfigurationWindow_Unloaded;
        }

        private async Task ShowProjectProblems()
        {
            this.StackPanelProblems.Children.Clear();

            var projectChecker = new CheckProjectIntegrity(_awbProject, Timelines);
            var problems = projectChecker.GetProblems().ToArray();
            foreach (var problem in problems)
            {
                var control = new System.Windows.Controls.Label { Content = problem.PlaintTextDescription, Foreground = new SolidColorBrush(Colors.Red) };
                this.StackPanelProblems.Children.Add(control);
            }

            await Task.CompletedTask;
        }

        #region Save Project Configuration

        private async Task<bool> SaveProjectConfigAsync()
        {
            _viewModel.WriteToProject(_awbProject);

            // check if project has errors
            var projectChecker = new CheckProjectIntegrity(_awbProject, Timelines);
            var problems = projectChecker.GetProblems().ToArray();
            var errors = problems.Where(p => p.ProblemType == ProjectProblem.ProblemTypes.Error).ToArray();
            if (errors.Any())
            {
                MessageBox.Show("Please fix all errors before saving the project configuration.\r\n\r\n" +
                    string.Join("\r\n", errors.Select(p => p.PlaintTextDescription)));
                return false;
            }

            var ok = await _projectManagerService.SaveProjectAsync(_awbProject, _awbProject.ProjectFolder);
            if (ok) _viewModel.UnsavedChanges = false; else MessageBox.Show("Error saving project configuration!");
            return ok;
        }

        private async void ProjectConfigurationWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_viewModel.UnsavedChanges == true)
            {
                var choice = MessageBox.Show("Save changes?", "Unsaved changes!", MessageBoxButton.YesNoCancel);
                switch (choice)
                {
                    case MessageBoxResult.Yes:
                        // save and close
                        if (await SaveProjectConfigAsync() == false) return;
                        break;
                    case MessageBoxResult.No:
                        // close without saving
                        break;
                    case MessageBoxResult.Cancel:
                        // cancel close
                        e.Cancel = true;
                        return;
                    default:
                        throw new ArgumentOutOfRangeException($"{nameof(choice)}:{choice}");
                }
            }
        }

        private void ButtonCloseWithoutSaving_Click(object sender, RoutedEventArgs e)
        {
            //todo: ask if unsaved changes should be saved
            if (_viewModel.UnsavedChanges == true)
            {
                var choice = MessageBox.Show("Save project config?", "Unsaved changes!", MessageBoxButton.YesNoCancel);
                switch (choice)
                {
                    case MessageBoxResult.Yes:
                        // save and close
                        if (SaveProjectConfigAsync().Result == false) return;
                        break;
                    case MessageBoxResult.No:
                        // close without saving
                        break;
                    case MessageBoxResult.Cancel:
                        // cancel close
                        return;
                    default:
                        throw new ArgumentOutOfRangeException($"{nameof(choice)}:{choice}");
                }
            }
            this.Close();
        }

        private async void ButtonSaveAndClose_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.UnsavedChanges == true)
            {
                if (await SaveProjectConfigAsync() == false) return;
            }
            this.Close();
        }

        #endregion

        #region add new objects buttons

        private void ButtonAddScsServo_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ScsServos.Add(new ScsFeetechServoConfig
            {
                Id = _idCreator.CreateNewObjectId("ScsServo"),
                Title = "",
                ClientId = 1,
                Channel = 1
            });
        }

        #endregion

        private void ProjectObjectSelected(object sender, ProjectObjectListControl.ProjectObjectSelectedEventArgs e)
        => SetObjectToEdit(e.ProjectObject);

        private void EditProjectMetaDataButton_Click(object sender, RoutedEventArgs e)
         => SetObjectToEdit(_viewModel.ProjectMetaData);

        private void EditEsp32HardwareButton_Click(object sender, RoutedEventArgs e)
        => SetObjectToEdit(_viewModel.Esp32ClientHardwareConfig);

        private async void SetObjectToEdit(IProjectObjectListable? projectObject)
        {
            await ShowProjectProblems();

            if (!await PropertyEditor.TrySetProjectObject(projectObject, _awbProject, Timelines, _invokerService.GetInvoker()))
            {
                projectObject = PropertyEditor.ProjectObject; // reject new project object fall back to the actual project object
                MessageBox.Show("Please fix property errors before changing active object.");
            }

            // iterator through all ProjectObjectListControl in this control and set the projectObject to each of them as selected object
            foreach (var control in StackPanelProjectObjectLists.Children)
                if (control is ProjectObjectListControl list)
                    list.SelectedProjectObject = projectObject;

            UpdateProjectObjectsListItemTitels();
        }


        // Update the listed item titles in die object lists
        private void UpdateProjectObjectsListItemTitels()
        {
            var childControls = StackPanelProjectObjectLists.Children;
            foreach (var control in childControls)
                if (control is ProjectObjectListControl list)
                    list.UpdateListItemTitels();
        }


        private void PropertyEditorUpdatedData_Fired(object? sender, EventArgs e)
        {
            UpdateProjectObjectsListItemTitels();
            _viewModel.UnsavedChanges = true;
        }

        private async void PropertyEditorOnObjectDelete_Fired(object? sender, DeleteObjectEventArgs e)
        {
            if (e.ObjectToDelete == _viewModel.ProjectMetaData)
            {
                MessageBox.Show("You can not delete the project meta data object!");
                return;
            }

            if (e.ObjectToDelete == _viewModel.Esp32ClientHardwareConfig)
            {
                MessageBox.Show("You can not delete the ESP32 hardware configuration object!");
                return;
            }

            _viewModel.Inputs.Remove(e.ObjectToDelete);
            _viewModel.TimelineStates.Remove(e.ObjectToDelete);
            _viewModel.ScsServos.Remove(e.ObjectToDelete);
            _viewModel.StsServos.Remove(e.ObjectToDelete);
            _viewModel.Mp3PlayerYX5300.Remove(e.ObjectToDelete);
            _viewModel.Pca9685PwmServos.Remove(e.ObjectToDelete);

            await PropertyEditor.TrySetProjectObject(projectObject: null, _awbProject, Timelines, _invokerService.GetInvoker());

            _viewModel.UnsavedChanges = true;
        }

        private void ScsServosList_NewProjectObjectRequested(object sender, EventArgs e)
        {
            var item = new ScsFeetechServoConfig
            {
                Id = _idCreator.CreateNewObjectId("ScsServo"),
                Title = "",
                ClientId = 1,
                Channel = 1
            };
            _viewModel.ScsServos.Add(item);
            ScsServosList.SelectedProjectObject = item;
        }
        

        private void StsServosList_NewProjectObjectRequested(object sender, EventArgs e)
        {
            var item =    new StsFeetechServoConfig
            {
                Id = _idCreator.CreateNewObjectId("StsServo"),
                Title = "",
                ClientId = 1,
                Channel = 1
            };
            _viewModel.StsServos.Add(item);
            StsServosList.SelectedProjectObject = item;
        }

        private void Pca9685PWMServosList_NewProjectObjectRequested(object sender, EventArgs e)
        {
            var item = new Pca9685PwmServoConfig
            {
                Id = _idCreator.CreateNewObjectId("Pca9685PwmServo"),
                I2cAdress = 0x40,
                Title = "",
                ClientId = 1,
                Channel = 1
            };
            _viewModel.Pca9685PwmServos.Add(item);
            Pca9685PWMServosList.SelectedProjectObject = item;
        }

        private void Mp3PlayerYX5300List_NewProjectObjectRequested(object sender, EventArgs e)
        {
            var item = new Mp3PlayerYX5300Config
            {
                Id = _idCreator.CreateNewObjectId("Mp3PlayerYX5300"),
                Title = "",
                ClientId = 1,
                RxPin = 13,
                TxPin = 14
            };
            _viewModel.Mp3PlayerYX5300.Add(item);
            Mp3PlayerYX5300List.SelectedProjectObject = item;
        }

        private void Mp3PlayerDFPlayerMiniList_NewProjectObjectRequested(object sender, EventArgs e)
        {
            var item = new Mp3PlayerDfPlayerMiniConfig
            {
                Id = _idCreator.CreateNewObjectId("Mp3PlayerDFPlayerMini"),
                Title = "",
                ClientId = 1,
                RxPin = 13,
                TxPin = 14,
                Volume = 10
            };
            _viewModel.Mp3PlayerDFPlayerMini.Add(item);
            Mp3PlayerDFPlayerMiniList.SelectedProjectObject = item;
        }

        private void InputsList_NewProjectObjectRequested(object sender, EventArgs e)
        {
            var item = new InputConfig
            {
                Id = _idCreator.GetNewInputId(),
                Title = "",
                ClientId = 1,
            };
            _viewModel.Inputs.Add(item);
            InputsList.SelectedProjectObject = item;
        }

        private void TimelineStates_NewProjectObjectRequested(object sender, EventArgs e)
        {
            var item = new TimelineState
            {
                Id = _idCreator.GetNewTimelineStateId(),
                Title = "",
                PositiveInputs = Array.Empty<int>(),
                NegativeInputs = Array.Empty<int>()
            };

            _viewModel.TimelineStates.Add(item);
            TimelineStatesList.SelectedProjectObject = item;

        }
    }
}

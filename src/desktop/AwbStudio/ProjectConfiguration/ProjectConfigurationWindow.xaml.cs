// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Project;
using Awb.Core.Project.Servos;
using Awb.Core.Timelines;
using AwbStudio.ProjectConfiguration;
using AwbStudio.ProjectConfiguration.PropertyEditors;
using AwbStudio.Projects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace AwbStudio
{
    /// <summary>
    /// Interaction logic for ProjectConfigurationWindow.xaml
    /// </summary>
    public partial class ProjectConfigurationWindow : Window
    {

        private ProjectConfigViewModel _viewModel;
        private readonly IProjectManagerService _projectManagerService;
        private readonly AwbProject _awbProject;

        private TimelineData[] _timelines;
        private TimelineData[] Timelines
        {
            get
            {
                if (_timelines == null)
                {
                    var ids = _awbProject.TimelineDataService.TimelineIds.ToArray();
                    var timelines = ids.Select(id => _awbProject.TimelineDataService.GetTimelineData(id))
                        .ToArray();
                    _timelines = timelines; 
                }
                return _timelines;
            }
        }

        public ProjectConfigurationWindow(IProjectManagerService projectManagerService)
        {
            _projectManagerService = projectManagerService;
            _awbProject = projectManagerService.ActualProject ?? throw new ArgumentNullException(nameof(projectManagerService.ActualProject));
            _viewModel = new ProjectConfigViewModel(_awbProject);

            this.DataContext = _viewModel;

            InitializeComponent();

            Loaded += ProjectConfigurationWindow_Loaded;
        }

        private void ProjectConfigurationWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // select the project properties as default
            SetObjectToEdit(_viewModel.ProjectMetaData);

            Loaded -= ProjectConfigurationWindow_Loaded;
            Closing += ProjectConfigurationWindow_Closing;
            Unloaded += ProjectConfigurationWindow_Unloaded;
        }
        private void ProjectConfigurationWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            Closing += ProjectConfigurationWindow_Closing;
            Unloaded -= ProjectConfigurationWindow_Unloaded;
        }

        #region Save Project Configuration

        private async Task<bool> SaveProjectConfigAsync()
        {
            _viewModel.WriteToProject(_awbProject);

            // check if project has errors
            var errors = _awbProject.GetProjectProblems(Timelines).Where(p => p.ProblemType == ProjectProblem.ProblemTypes.Error);
            if (errors.Any())
            {
                MessageBox.Show("Please fix all errors before saving the project configuration.\r\n\r\n"+
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
                Id = _awbProject.CreateNewObjectId("ScsServo"),
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

        private void SetObjectToEdit(IProjectObjectListable? projectObject)
        {
            if (!PropertyEditor.TrySetProjectObject(projectObject, _awbProject, Timelines))
            {
                projectObject = PropertyEditor.ProjectObject; // reject new project object fall back to the actual project object
                MessageBox.Show("Please fix property errors before changing active object.");
            }

            // iterator through all ProjectObjectListControl in this control and set the projectObject to each of them as selected object
            foreach (var control in StackPanelProjectObjectLists.Children)
                if (control is ProjectObjectListControl list)
                    list.SelectedProjectObject = projectObject;
        }

        private void UpdateProblemsDisplay()
        {
            var timelines = new List<TimelineData>();
            foreach (var projectObject in _awbProject.GetProjectProblems(timelines))
            {

            }
        }

        private void PropertyEditorUpdatedData_Fired(object? sender, EventArgs e)
        {
            _viewModel.UnsavedChanges = true;
        }

        private void PropertyEditorOnObjectDelete_Fired(object? sender, ProjectObjectGenericEditorControl.DeleteObjectEventArgs e)
        {
            if (e.ObjectToDelete == _viewModel.ProjectMetaData   )
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
            _viewModel.ScsServos.Remove(e.ObjectToDelete);
            _viewModel.StsServos.Remove(e.ObjectToDelete);
            _viewModel.Mp3PlayerYX5300.Remove(e.ObjectToDelete);
            _viewModel.Pca9685PwmServos.Remove(e.ObjectToDelete);
            _viewModel.UnsavedChanges = true;
        }
    }
}

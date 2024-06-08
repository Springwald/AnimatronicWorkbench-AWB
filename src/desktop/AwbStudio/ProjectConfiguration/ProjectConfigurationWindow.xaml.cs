// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Actuators;
using Awb.Core.Project;
using AwbStudio.ProjectConfiguration;
using AwbStudio.Projects;
using System;
using System.Collections.ObjectModel;
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
            _viewModel.ScsServos.Add(new StsServoConfig(id: "", title: "", clientId:1, channel: 1));
        }

        #endregion

        private void ProjectObjectSelected(object sender, ProjectObjectListControl.ProjectObjectSelectedEventArgs e)
        {
            SetObjectToEdit(e.ProjectObject);
        }

        private void EditProjectMetaDataButton_Click(object sender, RoutedEventArgs e)
        {
            SetObjectToEdit(_viewModel.ProjectMetaData);
        }

        private void EditEsp32HardwareButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SetObjectToEdit(IProjectObjectListable? projectObject)
        {
            // iterator through all ProjectObjectListControl in this control and set the projectObject to each of them as selected object
            foreach(var control in StackPanelProjectObjectLists.Children)
            {
                if (control is ProjectObjectListControl list)
                {
                    list.SelectedProjectObject = projectObject;
                }
            }

            PropertyEditor.ProjectObject = projectObject;
        }
    }
}

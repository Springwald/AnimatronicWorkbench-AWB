// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Project;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace AwbStudio.ProjectConfiguration
{
    /// <summary>
    /// Interaction logic for ProjectObjectListControl.xaml
    /// </summary>
    public partial class ProjectObjectListControl : UserControl
    {
        public class ProjectObjectSelectedEventArgs : EventArgs
        {
            public required IProjectObjectListable? ProjectObject { get; set; }
        }

        // the event, when a project object is selected
        public event EventHandler<ProjectObjectSelectedEventArgs>? ProjectObjectSelected;

        // the event, when a new project object is requested
        public event EventHandler? NewProjectObjectRequested;

        public ObservableCollection<IProjectObjectListable> ProjectObjects
        {
            get => (ObservableCollection<IProjectObjectListable>)GetValue(ProjectObjectsProperty);
            set { SetValue(ProjectObjectsProperty, value); }
        }

        public static readonly DependencyProperty ProjectObjectsProperty =
            DependencyProperty.Register(nameof(ProjectObjects), typeof(ObservableCollection<IProjectObjectListable>), typeof(ProjectObjectListControl), new PropertyMetadata(null));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public IProjectObjectListable? SelectedProjectObject
        {
            get => ListProjectObjects.SelectedItem as IProjectObjectListable;
            set
            {
                if (ListProjectObjects.Items.Contains(value))
                    ListProjectObjects.SelectedItem = value;
                else
                    ListProjectObjects.SelectedItem = null;
            }
        }

        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(ProjectObjectListControl), new PropertyMetadata(null));

        public ProjectObjectListControl()
        {
            InitializeComponent();
            Title = "Project Objects";
        }
        private void ListProjectObjects_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedProjectObject == null) return;
            ProjectObjectSelected?.Invoke(this,
                new ProjectObjectSelectedEventArgs { ProjectObject = this.SelectedProjectObject });
        }

        private void ButtonAddNew_Click(object sender, RoutedEventArgs e)
        {
            NewProjectObjectRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}

// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Project;
using Awb.Core.Tools.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace AwbStudio.ProjectConfiguration.PropertyEditors
{
    /// <summary>
    /// Interaction logic for ScsServoEditorControl.xaml
    /// </summary>
    public partial class ProjectObjectGenericEditorControl : UserControl
    {
        public class DeleteObjectEventArgs : EventArgs
        {
            public IProjectObjectListable ObjectToDelete { get; private set; }

            public DeleteObjectEventArgs(IProjectObjectListable objectToDelete)
            {
                ObjectToDelete = objectToDelete;
            }
        }

        private List<SinglePropertyEditorControl> _editors;
        private IProjectObjectListable _projectObject;
        private AwbProject _awbProject;
        private string[] _objectsUsingThisObject;

        public EventHandler OnUpdatedData { get;  set; }
        public EventHandler<DeleteObjectEventArgs> OnDeleteObject { get;  set; }

        public string? ActualProblems { get;  set; }

        public void SetProjectAndObject(IProjectObjectListable projectObject, AwbProject awbProject, string[] objectsUsingThisObject)
        {
            _projectObject = projectObject;
            _awbProject = awbProject;
            _objectsUsingThisObject = objectsUsingThisObject;
            WriteDataToEditor(projectObject);
            UpdateProblems();
        }
        
        public IProjectObjectListable ProjectObjectToEdit
        {
            get => _projectObject;
        }

        public ProjectObjectGenericEditorControl()
        {
            InitializeComponent();
            this.DataContext = ProjectObjectToEdit;
        }

        private void WriteDataToEditor(IProjectObjectListable stsServoConfig)
        {
            if (_editors != null) throw new System.Exception("Object to edit is already set.");

            var type = stsServoConfig.GetType();
            _editors = new List<SinglePropertyEditorControl>();

            // iterate trough all properties  of the object which have a DataAnnotation for "Name" and add an editor for each
            foreach (var property in type.GetProperties())
            {
                // check if the property has a name set via DisplayName Annotation
                var displayNameAttribute = property.GetCustomAttribute<DisplayNameAttribute>();
                if (displayNameAttribute == null) continue;

                var editor = new SinglePropertyEditorControl();
                //editor.SetPropertyToEditByExpression(() => stsServoConfig.GetType().GetProperty(property.Name));
                editor.SetPropertyToEditByName(target: stsServoConfig, propertyName: property.Name);
                editor.PropertyChanged += (s, e) =>
                {
                    LabelObjectTypeTitle.Content = stsServoConfig.TitleDetailed;
                    this.UpdateProblems();
                    this.OnUpdatedData?.Invoke(this, EventArgs.Empty);
                };
                _editors.Add(editor);
                this.EditorStackPanel.Children.Add(editor);
            }

            // find objects using this object
            if (_objectsUsingThisObject.Any())
            {
                TextUsageIn.Text = $"Used {_objectsUsingThisObject.Length} times (mouse over for details)"; 
                TextUsageIn.ToolTip = string.Join(",\r\n", _objectsUsingThisObject);
                TextUsageIn.Visibility = System.Windows.Visibility.Visible;
            }
            else
                TextUsageIn.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void UpdateProblems()
        {
            var problemsText = new StringBuilder();

            const bool showAlsoNativeErrors = false; // true = usually results in duplicate error entries
            if (showAlsoNativeErrors)
            {
#pragma warning disable CS0162 // Unreachable code detected
                var nativeErrors = ObjectValidator.ValidateObjectGetErrors(_projectObject);

                foreach (var error in nativeErrors)
                    problemsText.AppendLine("Native error:" + error.PlaintTextDescription);
#pragma warning restore CS0162 // Unreachable code detected
            }

            foreach (var problem in _projectObject.GetContentProblems(_awbProject))
                problemsText.AppendLine(problem.PlaintTextDescription);

            foreach (var editor in _editors)
            {
                var errors = editor.ErrorMessagesJoined;
                if (!string.IsNullOrWhiteSpace(errors))
                    problemsText.AppendLine($"{editor.PropertyTitle}:{errors}");
            }

            if (problemsText.Length == 0)
            {
                ActualProblems = null;
                TextProblems.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                ActualProblems = problemsText.Length == 0 ? null : problemsText.ToString();
                TextProblems.Text = ActualProblems ?? string.Empty;
                TextProblems.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void ButtonDelete_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_objectsUsingThisObject.Any())
            {
                MessageBox.Show($"This object is used in the following objects:\r\n{string.Join(",\r\n", _objectsUsingThisObject)}\r\n\r\nPlease remove the object from these usages first.", "Object is used", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // throw delete event
            if (MessageBox.Show($"Do you really want to delete the object '{_projectObject.TitleDetailed}'?", "Delete object", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                this.OnDeleteObject?.Invoke(this,new DeleteObjectEventArgs(_projectObject));
            }

        }
    }
}

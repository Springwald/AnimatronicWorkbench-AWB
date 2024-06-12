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
using System.Reflection;
using System.Text;
using System.Windows.Controls;

namespace AwbStudio.ProjectConfiguration.PropertyEditors
{
    /// <summary>
    /// Interaction logic for ScsServoEditorControl.xaml
    /// </summary>
    public partial class ProjectObjectGenericEditorControl : UserControl
    {
        private List<SinglePropertyEditorControl> _editors;
        private IProjectObjectListable _projectObject;
        private AwbProject _awbProject;

        public EventHandler OnUpdatedData { get; private set; }

        public string? ActualProblems { get; private set; }

        public void SetProjectAndObject(IProjectObjectListable projectObject, AwbProject awbProject)
        {
            _projectObject = projectObject;
            _awbProject = awbProject;
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

            //editor.SetPropertyToEditByExpression(() => stsServoConfig.ClientId);
            //editor.SetPropertyToEditByName(stsServoConfig, "ClientId");
            //_editors.Add(editor);
            //this.EditorStackPanel.Children.Add(editor);

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
    }
}

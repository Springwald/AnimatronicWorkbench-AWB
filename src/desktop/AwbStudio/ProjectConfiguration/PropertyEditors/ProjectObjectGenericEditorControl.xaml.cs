// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Project;
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
        private List<ValueEditorControl> _editors;
        private IProjectObjectListable _projectObject;

        public EventHandler OnUpdatedData { get; private set; }

        public IProjectObjectListable ProjectObjectToEdit
        {
            get
            {
                ReadDataFromEditor(_projectObject);
                return _projectObject;
            }
            set
            {
                _projectObject = value;
                WriteDataToEditor(value);
                UpdateProblems();
            }
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
            _editors = new List<ValueEditorControl>();

            // iterate trough all properties  of the object which have a DataAnnotation for "Name" and add an editor for each
            foreach (var property in type.GetProperties())
            {
                // check if the property has a name set via DisplayName Annotation
                var displayNameAttribute = property.GetCustomAttribute<DisplayNameAttribute>();
                if (displayNameAttribute == null) continue;

                var editor = new ValueEditorControl();
                //editor.SetPropertyToEditByExpression(() => stsServoConfig.GetType().GetProperty(property.Name));
                editor.SetPropertyToEditByName(target: stsServoConfig, propertyName: property.Name);
                editor.PropertyChanged += (s, e) =>
                {
                    LabelObjectTypeTitle.Content = stsServoConfig.TitleDetailled;
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

            foreach (var problem in _projectObject.GetProblems(null))
            {
                problemsText.AppendLine(problem.Message);
            }

            if (problemsText.Length == 0)
            {
                TextProblems.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                TextProblems.Text = problemsText.ToString();
                TextProblems.Visibility = System.Windows.Visibility.Visible;
            }


        }

        private void ReadDataFromEditor(IProjectObjectListable scsServoConfig)
        {

        }


    }
}

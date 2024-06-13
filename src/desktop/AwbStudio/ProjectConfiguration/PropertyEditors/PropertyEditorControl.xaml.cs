// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Project;
using Awb.Core.Project.Servos;
using Awb.Core.Timelines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using static AwbStudio.ProjectConfiguration.PropertyEditors.ProjectObjectGenericEditorControl;

namespace AwbStudio.ProjectConfiguration.PropertyEditors
{
    /// <summary>
    /// Interaction logic for PropertyEditorControl.xaml
    /// </summary>
    public partial class PropertyEditorControl : UserControl
    {
        private ProjectObjectGenericEditorControl? _actualEditor;
        private IProjectObjectListable? _projectObject;

        public EventHandler OnUpdatedData { get; set; }
        public EventHandler<DeleteObjectEventArgs> OnDeleteObject { get; set; }

        private ProjectObjectGenericEditorControl? ActualEditor
        {
            get => _actualEditor;
            set
            {
                if (_actualEditor != null)
                {
                    // remove an existing editor first
                    PropertyEditorGrid.Children.Clear();
                }
                _actualEditor = value;
                PropertyEditorGrid.Children.Add(_actualEditor);
            }
        }

        public IProjectObjectListable? ProjectObject
        {
            get => _projectObject;
        }

        public bool TrySetProjectObject(IProjectObjectListable? projectObject, AwbProject awbProject, TimelineData[] timelines)
        {
            if (_projectObject != projectObject)
            {
                if (projectObject == null)
                {
                    ActualEditor = null;
                }
                else
                {
                    // Check if there are problems with the object in the editor
                    if (ActualEditor != null && ActualEditor.ActualProblems != null) return false;

                    // instanciate the suiting editor control for the object type using switch
                    // e.g. ScsServoEditorControl for ScsServoConfig
                    var objectsUsingThisObject = new List<string>();
                    switch (projectObject)
                    {
                        case FeetechBusServoConfig feetechServoConfig:
                            objectsUsingThisObject.AddRange(
                                timelines.Where(t => t.ServoPoints.Any(p => p.ServoId == feetechServoConfig.Id)).Select(t => $"Timeline '{t.Title}'"));
                            break;

                        default:
                            break;
                    }



                    var editor = new ProjectObjectGenericEditorControl();
                    editor.SetProjectAndObject(projectObject, awbProject, objectsUsingThisObject.ToArray());
                    ActualEditor = editor;
                    ActualEditor.OnDeleteObject += OnObjectDelete_Fired;
                    ActualEditor.OnUpdatedData += UpdatedData_Fired;
                }
                _projectObject = projectObject;
            }
            return true;
        }



        public PropertyEditorControl()
        {
            InitializeComponent();
        }

        private void UpdatedData_Fired(object? sender, EventArgs e)
            => OnUpdatedData?.Invoke(this, new EventArgs());

        private void OnObjectDelete_Fired(object? sender, ProjectObjectGenericEditorControl.DeleteObjectEventArgs e)
            => OnDeleteObject?.Invoke(this, e);
    }
}

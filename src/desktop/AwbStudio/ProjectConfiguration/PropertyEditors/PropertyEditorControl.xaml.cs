// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Project;
using System.Windows.Controls;

namespace AwbStudio.ProjectConfiguration.PropertyEditors
{
    /// <summary>
    /// Interaction logic for PropertyEditorControl.xaml
    /// </summary>
    public partial class PropertyEditorControl : UserControl
    {
        private ProjectObjectGenericEditorControl? _actualEditor;
        private IProjectObjectListable? _projectObject;

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

        public bool TrySetProjectObject(IProjectObjectListable? projectObject)
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
                    if (ActualEditor!= null && ActualEditor.ActualProblems != null) return false;

                    // instanciate the suiting editor control for the object type using switch
                    // e.g. ScsServoEditorControl for ScsServoConfig
                    switch (_projectObject)
                    {
                        // case ScsFeetechServoConfig scsServoConfig:
                        //     break;
                        default:
                            ActualEditor = new ProjectObjectGenericEditorControl() { ProjectObjectToEdit = projectObject };
                            break;
                    }
                }
                _projectObject = projectObject;
            }
            return true;
        }

        public PropertyEditorControl()
        {
            InitializeComponent();
        }
    }
}

﻿// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Project;
using Awb.Core.Project.Servos;
using Awb.Core.Project.Various;
using System;
using System.Windows.Controls;

namespace AwbStudio.ProjectConfiguration.PropertyEditors
{
    /// <summary>
    /// Interaction logic for PropertyEditorControl.xaml
    /// </summary>
    public partial class PropertyEditorControl : UserControl
    {
        private UserControl? _actualEditor;
        private IProjectObjectListable? _projectObject;



        private UserControl? ActualEditor
        {
            get => _actualEditor;
            set
            {
                if (_actualEditor != null)
                {
                    // remove an existing editor first
                    PropertyEditorScrollViewer.Content = null;
                }
                _actualEditor = value;
                PropertyEditorScrollViewer.Content = _actualEditor;
            }
        }

        public IProjectObjectListable? ProjectObject
        {
            get => _projectObject;
            set
            {

                if (_projectObject != value)
                {
                    _projectObject = value;

                    if (_projectObject == null)
                    {
                        ActualEditor = null;
                    }
                    else
                    {

                        // instanciate the suiting editor control for the object type using switch
                        // e.g. ScsServoEditorControl for ScsServoConfig
                        switch (_projectObject)
                        {
                            case ScsFeetechServoConfig scsServoConfig:
                                ActualEditor = new ScsServoEditorControl() { ScsServoConfig = scsServoConfig };
                                break;

                            case ProjectMetaData projectMetaData:
                                ActualEditor = new ScsServoEditorControl() {  ScsServoConfig  = projectMetaData };
                                break;

                            default:
                                throw new ArgumentOutOfRangeException(nameof(ProjectObject) + ":" + _projectObject.ToString());
                        }
                    }
                }
            }
        }

        public PropertyEditorControl()
        {
            InitializeComponent();
        }
    }
}

// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Project;
using Awb.Core.Project.Servos;
using Awb.Core.Project.Various;
using Awb.Core.Services;
using Awb.Core.Timelines;
using Awb.Core.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
                if (_actualEditor != null)
                    PropertyEditorGrid.Children.Add(_actualEditor);
            }
        }

        public IAwbClientsService AwbClientsService { get; set; }

        public IProjectObjectListable? ProjectObject
        {
            get => _projectObject;
        }

        public async Task<bool> TrySetProjectObject(IProjectObjectListable? projectObject, AwbProject awbProject, TimelineData[] timelines, IInvoker invoker)
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
                        /*  Servos */

                        case FeetechBusServoConfig feetechServoConfig:
                            objectsUsingThisObject.AddRange(
                                timelines.Where(t => t.ServoPoints.Any(p => p.ServoId == feetechServoConfig.Id)).Select(t => $"Timeline '{t.Title}'"));
                            break;

                        case Pca9685PwmServoConfig pca9685PwmServoConfig:
                            objectsUsingThisObject.AddRange(
                                timelines.Where(t => t.ServoPoints.Any(p => p.ServoId == pca9685PwmServoConfig.Id)).Select(t => $"Timeline '{t.Title}'"));
                            break;

                        case DynamixelBusServoConfig dynamixelBusServoConfig:
                            objectsUsingThisObject.AddRange(
                                timelines.Where(t => t.ServoPoints.Any(p => p.ServoId == dynamixelBusServoConfig.Id)).Select(t => $"Timeline '{t.Title}'"));
                            break;

                        /* MP3-Player */

                        case Mp3PlayerDfPlayerMiniConfig mp3PlayerDfPlayerMiniConfig:
                            objectsUsingThisObject.AddRange(
                                timelines.Where(t => t.SoundPoints.Any(p => p.SoundPlayerId == mp3PlayerDfPlayerMiniConfig.Id)).Select(t => $"Timeline '{t.Title}'"));
                            break;

                        case Mp3PlayerYX5300Config mp3PlayerYX5300Config:
                            objectsUsingThisObject.AddRange(
                                timelines.Where(t => t.SoundPoints.Any(p => p.SoundPlayerId == mp3PlayerYX5300Config.Id)).Select(t => $"Timeline '{t.Title}'"));
                            break;

                        default:
                            break;
                    }

                    if (ActualEditor != null)
                    {
                        ActualEditor.OnDeleteObject -= OnObjectDelete_Fired;
                        ActualEditor.OnUpdatedData -= UpdatedData_Fired;
                    }

                    var editor = new ProjectObjectGenericEditorControl(AwbClientsService);
                    await editor.SetProjectAndObject(projectObject, awbProject, invoker, objectsUsingThisObject.ToArray());
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

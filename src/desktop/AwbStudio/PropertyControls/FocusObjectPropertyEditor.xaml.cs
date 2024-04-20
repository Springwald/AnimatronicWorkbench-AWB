// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Accessibility;
using Awb.Core.Actuators;
using Awb.Core.ActuatorsAndObjects;
using Awb.Core.Player;
using Awb.Core.Sounds;
using Awb.Core.Timelines;
using AwbStudio.FileManagement;
using AwbStudio.TimelineControls.PropertyControls;
using AwbStudio.TimelineEditing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using System.Windows.Controls;

namespace AwbStudio.PropertyControls
{
    /// <summary>
    /// Interaction logic for FocusObjectPropertyEditor.xaml
    /// </summary>
    public partial class FocusObjectPropertyEditor : UserControl
    {
        private IServiceProvider _serviceProvider;
        private Sound[]? _projectSounds;
        private TimelineFileManager _timelineFileManager;
        private TimelineEditingManipulation? _timelineEditingManipulation;
        private TimelineData _timelineData;
        private TimelineViewContext? _viewContext;
        private PlayPosSynchronizer? _playPosSynchronizer;
        private IAwbObject? _focusObject;
        private IPropertyEditor? _actualPropertyEditor;
        private IPropertyEditorVirtualInputController _propertyEditorVirtualInputController;
        private bool _initialized;

        public FocusObjectPropertyEditor()
        {
            InitializeComponent();
            Unloaded += FocusObjectPropertyEditor_Unloaded;
        }

        private void FocusObjectPropertyEditor_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_viewContext != null)
                _viewContext.Changed -= ViewContext_Changed;

            RemoveEditor();
        }

        public void Init(IServiceProvider serviceProvider, TimelineViewContext timelineViewContext, TimelineData timelineData, TimelineEditingManipulation timelineEditingManipulation, PlayPosSynchronizer playPosSynchronizer, TimelineFileManager timelineFileManager, Sound[] projectSounds )
        {
            _serviceProvider = serviceProvider;
            _projectSounds = projectSounds;
            _timelineFileManager = timelineFileManager;
            _timelineEditingManipulation = timelineEditingManipulation;
            _timelineData = timelineData;
            _propertyEditorVirtualInputController = serviceProvider.GetRequiredService<IPropertyEditorVirtualInputController>();
            _viewContext = timelineViewContext;
            _viewContext.Changed += ViewContext_Changed;
            _playPosSynchronizer = playPosSynchronizer;
            _initialized = true;
        }


        private void ViewContext_Changed(object? sender, ViewContextChangedEventArgs e)
        {
            if (_initialized == false) return;
            if (sender == this) return;

            switch (e.ChangeType)
            {
                case ViewContextChangedEventArgs.ChangeTypes.FocusObject:
                    if (_viewContext != null && _viewContext.ActualFocusObject != _actualPropertyEditor?.AwbObject)
                    {
                        RemoveEditor();
                        _focusObject = _viewContext.ActualFocusObject;

                        if (_focusObject == null)
                        {
                            LabelPropertyEditorTitle.Content = "Properties [nothing selected]";
                            return;
                        }

                        LabelPropertyEditorTitle.Content = $"Properties for {_focusObject.Title}";

                        // cast the focus object to a property editor
                        if (_focusObject is IServo servo)
                        {
                            _actualPropertyEditor = new ServoPropertiesControl(servo,_timelineData, _viewContext, _playPosSynchronizer);
                            this.PropertyEditorGrid.Children.Clear();
                            this.PropertyEditorGrid.Children.Add(_actualPropertyEditor as UserControl);
                        }

                        if (_focusObject is ISoundPlayer soundPlayer)
                        {
                            _actualPropertyEditor = new SoundPlayerPropertyControl(soundPlayer,_timelineData,  _projectSounds);
                            this.PropertyEditorGrid.Children.Clear();
                            this.PropertyEditorGrid.Children.Add(_actualPropertyEditor as UserControl);
                        }

                        if (_focusObject == NestedTimelinesFakeObject.Singleton)
                        {
                            var editor = new NestedTimelinePropertyControl();
                            _actualPropertyEditor = editor;
                            editor.FileManager = _timelineFileManager;
                        }
                    }
                    break;

                case ViewContextChangedEventArgs.ChangeTypes.Duration:
                case ViewContextChangedEventArgs.ChangeTypes.BankIndex:
                case ViewContextChangedEventArgs.ChangeTypes.PixelPerMs:
                case ViewContextChangedEventArgs.ChangeTypes.Scroll:
                case ViewContextChangedEventArgs.ChangeTypes.FocusObjectValue:
                    break;

                default:
                    throw new System.ArgumentOutOfRangeException($"{nameof(e.ChangeType)}:{e.ChangeType}");
            }
        }

        private void OnValueChanged(object? sender, EventArgs e)
        {
            if (_viewContext != null && _actualPropertyEditor != null && _actualPropertyEditor.AwbObject == _viewContext?.ActualFocusObject)
            {
                _viewContext.FocusObjectValueChanged(this);
            }
        }

        private void RemoveEditor()
        {
            PropertyEditorGrid.Children.Clear();
            if (_actualPropertyEditor != null)
            {
                _actualPropertyEditor = null;
            }
        }


    }
}

// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Actuators;
using Awb.Core.ActuatorsAndObjects;
using Awb.Core.Player;
using Awb.Core.Project.Servos;
using Awb.Core.Project.Various;
using Awb.Core.Services;
using Awb.Core.Sounds;
using Awb.Core.Timelines;
using AwbStudio.TimelineControls;
using AwbStudio.TimelineEditing;
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
        private Sound[]? _projectSounds;
        private IServo[]? _projectServos;
        private SoundPlayerControl _windowsSoundPlayerControl;
        private ITimelineDataService? _timelineDataService;
        private TimelineEventHandling _timelineEventHandling;
        private TimelineData? _timelineData;
        private TimelineViewContext? _viewContext;
        private PlayPosSynchronizer? _playPosSynchronizer;
        private IAwbObject? _focusObject;
        private IPropertyEditor? _actualPropertyEditor;
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

        public void Init(
            TimelineViewContext timelineViewContext, 
            TimelineData timelineData, 
            TimelineEventHandling timelineEventHandling, 
            PlayPosSynchronizer playPosSynchronizer, 
            ITimelineDataService timelineDataService, 
            Sound[] projectSounds, 
            IServo[] projectServos, 
            SoundPlayerControl windowsSoundPlayerControl)
        {
            _projectSounds = projectSounds;
            _projectServos = projectServos;
            _windowsSoundPlayerControl = windowsSoundPlayerControl;
            _timelineDataService = timelineDataService;
            _timelineEventHandling = timelineEventHandling;
            _timelineData = timelineData;
            _viewContext = timelineViewContext;
            if (!_initialized) _viewContext.Changed += ViewContext_Changed;
            _playPosSynchronizer = playPosSynchronizer;
            if (_actualPropertyEditor != null)       RemoveEditor();
            _initialized = true;
        }

        private void ViewContext_Changed(object? sender, ViewContextChangedEventArgs e)
        {
            if (_initialized == false) 
                return;
            if (sender == this) return;

            switch (e.ChangeType)
            {
                case ViewContextChangedEventArgs.ChangeTypes.FocusObject:
                    //if (_viewContext.ActualFocusObject != _actualPropertyEditor?.AwbObject)
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
                            _actualPropertyEditor = new ServoPropertiesControl(servo, _timelineData!, _timelineEventHandling.TimelineEditingManipulation, _viewContext, _playPosSynchronizer!);
                            this.PropertyEditorGrid.Children.Clear();
                            this.PropertyEditorGrid.Children.Add(_actualPropertyEditor as UserControl);
                        }

                        if (_focusObject is ISoundPlayer soundPlayer)
                        {
                            _actualPropertyEditor = new SoundPlayerPropertyControl(soundPlayer, _projectSounds!, _projectServos!, _timelineData!, _viewContext, _playPosSynchronizer!, _windowsSoundPlayerControl);
                            this.PropertyEditorGrid.Children.Clear();
                            this.PropertyEditorGrid.Children.Add(_actualPropertyEditor as UserControl);
                        }

                        if (_focusObject == NestedTimelinesFakeObject.Singleton)
                        {
                            _actualPropertyEditor = new NestedTimelinePropertyControl(_timelineDataService!.TimelineMetaDataService, _timelineData!, _viewContext, _playPosSynchronizer!);
                            this.PropertyEditorGrid.Children.Clear();
                            this.PropertyEditorGrid.Children.Add(_actualPropertyEditor as UserControl);
                        }
                    }
                    break;

                case ViewContextChangedEventArgs.ChangeTypes.Duration:
                case ViewContextChangedEventArgs.ChangeTypes.BankIndex:
                case ViewContextChangedEventArgs.ChangeTypes.PixelPerMs:
                case ViewContextChangedEventArgs.ChangeTypes.Scroll:
                case ViewContextChangedEventArgs.ChangeTypes.FocusObjectValue:
                case ViewContextChangedEventArgs.ChangeTypes.Selection:
                    break;

                default:
                    throw new System.ArgumentOutOfRangeException($"{nameof(e.ChangeType)}:{e.ChangeType}");
            }
        }

        private void OnValueChanged(object? sender, EventArgs e)
        {
            if (_viewContext != null && _actualPropertyEditor != null && _actualPropertyEditor.AwbObject == _viewContext?.ActualFocusObject)
            {
                _viewContext!.FocusObjectValueChanged(this);
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

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
        private TimelineEditingManipulation? _timelineEditingManipulation;
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

            if (_playPosSynchronizer != null)
                _playPosSynchronizer.OnPlayPosChanged -= PlayPosSynchronizer_OnPlayPosChanged;

            RemoveEditor();
        }

        public void Init(IServiceProvider serviceProvider, TimelineViewContext timelineViewContext, TimelineEditingManipulation timelineEditingManipulation, PlayPosSynchronizer playPosSynchronizer, Sound[] projectSounds )
        {
            _serviceProvider = serviceProvider;
            _projectSounds = projectSounds;
            _timelineEditingManipulation = timelineEditingManipulation;
            _propertyEditorVirtualInputController = serviceProvider.GetRequiredService<IPropertyEditorVirtualInputController>();
            _viewContext = timelineViewContext;
            _viewContext.Changed += ViewContext_Changed;
            _playPosSynchronizer = playPosSynchronizer;
            _playPosSynchronizer.OnPlayPosChanged += PlayPosSynchronizer_OnPlayPosChanged;
            _initialized = true;
        }

        private async void PlayPosSynchronizer_OnPlayPosChanged(object? sender, int e)
        {
            if (_actualPropertyEditor != null)
                await _actualPropertyEditor.UpdateValue();
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
                            _actualPropertyEditor = new ServoPropertiesControl(servo,_timelineEditingManipulation);
                            _actualPropertyEditor.OnValueChanged += OnValueChanged;
                            this.PropertyEditorGrid.Children.Clear();
                            this.PropertyEditorGrid.Children.Add(_actualPropertyEditor as UserControl);
                        }

                        if (_focusObject is ISoundPlayer soundPlayer)
                        {
                            _actualPropertyEditor = new SoundPlayerPropertyControl(soundPlayer, _projectSounds);
                            _actualPropertyEditor.OnValueChanged += OnValueChanged;
                            this.PropertyEditorGrid.Children.Clear();
                            this.PropertyEditorGrid.Children.Add(_actualPropertyEditor as UserControl);
                        }
                    }
                    break;

                case ViewContextChangedEventArgs.ChangeTypes.Duration:
                case ViewContextChangedEventArgs.ChangeTypes.BankIndex:
                case ViewContextChangedEventArgs.ChangeTypes.PixelPerMs:
                case ViewContextChangedEventArgs.ChangeTypes.Scroll:
                    break;

                case ViewContextChangedEventArgs.ChangeTypes.FocusObjectValue:
                    if (_actualPropertyEditor != null && _viewContext?.ActualFocusObject == _actualPropertyEditor.AwbObject)
                        _actualPropertyEditor.UpdateValue();
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
                _actualPropertyEditor.OnValueChanged -= OnValueChanged;
                _actualPropertyEditor = null;
            }
        }


    }
}

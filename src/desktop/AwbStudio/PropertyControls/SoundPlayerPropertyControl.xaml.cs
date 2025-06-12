// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Actuators;
using Awb.Core.ActuatorsAndObjects;
using Awb.Core.Player;
using Awb.Core.Project.Various;
using Awb.Core.Sounds;
using Awb.Core.Timelines;
using AwbStudio.TimelineControls;
using AwbStudio.TimelineEditing;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AwbStudio.PropertyControls
{
    /// <summary>
    /// Interaction logic for SoundPlayerPropertyControl.xaml
    /// </summary>
    public partial class SoundPlayerPropertyControl : UserControl, IPropertyEditor
    {
        private const string NoServoTitle = "-- NO SERVO --";
        private const string NoSoundTitle = "-- NO SOUND --";
        private readonly IServo[] _servos;
        private readonly ISoundPlayer _soundPlayer;
        private readonly TimelineData _timelineData;
        private readonly Sound[] _projectSounds;
        private readonly SoundPlayerControl _windowsSoundPlayerControl;
        private readonly TimelineViewContext _viewContext;
        private readonly PlayPosSynchronizer _playPosSynchronizer;
        private bool _isUpdatingView;
        private bool _isSettingNewValue;

        public IAwbObject AwbObject => _soundPlayer;

        public SoundPlayerPropertyControl(ISoundPlayer soundPlayer, Sound[] projectSounds, IServo[] servos, TimelineData timelineData, TimelineViewContext viewContext, PlayPosSynchronizer playPosSynchronizer, SoundPlayerControl windowsSoundPlayerControl)
        {
            InitializeComponent();
            _servos = servos ?? throw new ArgumentNullException(nameof(servos), "Servos cannot be null or empty.");
            _soundPlayer = soundPlayer;
            _projectSounds = projectSounds;
            _windowsSoundPlayerControl = windowsSoundPlayerControl;

            _timelineData = timelineData;
            _timelineData.OnContentChanged += TimelineData_OnContentChanged;

            _viewContext = viewContext;
            _viewContext.Changed += ViewContext_Changed;

            _playPosSynchronizer = playPosSynchronizer;
            _playPosSynchronizer.OnPlayPosChanged += OnPlayPosChanged;

            //LabelName.Content = "MP3 " + soundPlayer.Title;

            Loaded += SoundPlayerPropertyControl_Loaded;
        }

        private void SoundPlayerPropertyControl_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= SoundPlayerPropertyControl_Loaded;
            Unloaded += SoundPlayerPropertiesControl_Unloaded;

            // Fill sounds
            ComboBoxSoundToPlay.Items.Add(NoSoundTitle);
            foreach (var sound in _projectSounds)
                ComboBoxSoundToPlay.Items.Add(sound.Title);

            // Fill movement servos
            ComboBoxServoToMove.Items.Add(NoServoTitle);
            foreach (var servo in _servos)
            {
                ComboBoxServoToMove.Items.Add(servo.Title);
            }

            ShowActualValue();
        }

        private void SoundPlayerPropertiesControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Unloaded -= SoundPlayerPropertiesControl_Unloaded;
            _timelineData.OnContentChanged -= TimelineData_OnContentChanged;
            _playPosSynchronizer.OnPlayPosChanged -= OnPlayPosChanged;
            _viewContext.Changed -= ViewContext_Changed;
        }

        private void ViewContext_Changed(object? sender, ViewContextChangedEventArgs e)
        {
            switch (e.ChangeType)
            {
                case ViewContextChangedEventArgs.ChangeTypes.FocusObject:
                case ViewContextChangedEventArgs.ChangeTypes.FocusObjectValue:
                    if (_viewContext.ActualFocusObject == _soundPlayer || _viewContext.ActualFocusObject == NestedTimelinesFakeObject.Singleton)
                        ShowActualValue();
                    break;
            }
        }

        private void TimelineData_OnContentChanged(object? sender, TimelineDataChangedEventArgs e)
        {
            if (e.ChangedObjectId == _soundPlayer.Id)
                ShowActualValue();
        }

        private void OnPlayPosChanged(object? sender, int e)
        {
            if (_viewContext.ActualFocusObject == _soundPlayer)
                ShowActualValue();
        }

      
        private void ValueChoiceChanged()
        {
            if (_isUpdatingView) return;

            var movementInverted = CheckBoxInvertMovement.IsChecked ?? false;
            var movementServoTitle = ComboBoxServoToMove.SelectedItem as string;
            string? movementServoId = null;
            if (movementServoTitle != null && movementServoTitle != NoServoTitle)
            {
                foreach (var servo in _servos)
                {
                    if (servo.Title == movementServoTitle)
                    {
                        movementServoId = servo.Id;
                        break;
                    }
                }
            }

            var index = ComboBoxSoundToPlay.SelectedIndex;
            var movementOffsetMs = (int)SliderMovementOffsetMs.Value;
            var movementValueScale = (int)SliderMovementValueScale.Value;
            if (index <= 0)
            {
                SetNewSoundValue(sound: null, actuatorMovementsBySound: []);
            }
            else
            {
                if (_projectSounds.Length < index)
                {
                    MessageBox.Show("Sound index " + index + " not found!");
                    return;
                }
                var newSound = _projectSounds[index - 1];

                var actuatorMovementsBySound = new ActuatorMovementBySound[]
                {
                    new ActuatorMovementBySound {
                        ActuatorId = movementServoId,
                        MovementInverted = movementInverted,
                        MovementOffsetMs = movementOffsetMs,
                        MovementValueScale = movementValueScale,
                        MovementFrequencyMs = 50, // default values for movement offset and frequency
                    }
                };

                SetNewSoundValue(newSound, actuatorMovementsBySound);
            }
        }

        private void SetNewSoundValue(Sound? sound, ActuatorMovementBySound[] actuatorMovementsBySound)
        {
            _isSettingNewValue = true;

            var changed = false;

            if (_soundPlayer.ActualSoundId != sound?.Id)
            {
                if (sound == null)
                {
                    _soundPlayer.SetActualSoundId(null, TimeSpan.Zero);
                }
                else
                {
                    _soundPlayer.SetActualSoundId(sound!.Id, TimeSpan.Zero);
                    _windowsSoundPlayerControl.PlaySound(this, new SoundPlayEventArgs(sound!.Id, startTime: TimeSpan.Zero));
                }
                changed = true;
            }

            // todo: allow multiple movements per sound in the future; at the moment we only support one movement per sound
            if (ActuatorMovementBySound.AreEqual(_soundPlayer.ActuatorMovementsBySound, actuatorMovementsBySound) == false)
            {
                if (sound == null)
                {
                    _soundPlayer.SetActuatorMovementBySound([]);
                }
                else
                {
                    _soundPlayer.SetActuatorMovementBySound(actuatorMovementsBySound);
                }
                changed = true;
            }

            if (changed)
            {
                _soundPlayer.IsDirty = true;
                _viewContext.FocusObjectValueChanged(this);
            }

            _isSettingNewValue = false;
            ShowActualValue();
        }

        private void ShowActualValue()
        {
            if (_isSettingNewValue) return;

            _isUpdatingView = true;

            var soundId = _soundPlayer.ActualSoundId;

            var labelValueScale = "Movement value scale";
            var labelValueOffset = "Movement offset";

            if (soundId == null)
            {
                EnableEditorControls(servoChoiceEnabled: false, propertiesEnabled: false);
            }
            else
            {
                for (int index = 0; index < _projectSounds.Length; index++)
                {
                    if (_projectSounds[index].Id == soundId)
                    {
                        ComboBoxSoundToPlay.SelectedIndex = index + 1;
                        break; ;
                    }
                }

                ComboBoxServoToMove.IsEnabled = true;

                var firstActuatorMovementBySound = _soundPlayer.ActuatorMovementsBySound.FirstOrDefault(); // todo: at the moment we only support one movement per sound

                if (firstActuatorMovementBySound == null)
                {
                    // no movement defined for this sound
                    EnableEditorControls(servoChoiceEnabled: true, propertiesEnabled: false);
                }
                else
                {
                    // a movement is defined for this sound
                    string? actualMovementServoTitle = null;
                    foreach (var servo in _servos)
                    {
                        if (servo.Id == firstActuatorMovementBySound.ActuatorId)
                        {
                            actualMovementServoTitle = servo.Title;
                            break;
                        }
                    }

                    EnableEditorControls(servoChoiceEnabled: true, propertiesEnabled: true);
                    ComboBoxServoToMove.SelectedItem = actualMovementServoTitle ?? NoServoTitle;
                    CheckBoxInvertMovement.IsChecked = firstActuatorMovementBySound.MovementInverted;
                    SliderMovementOffsetMs.Value = firstActuatorMovementBySound.MovementOffsetMs;
                    SliderMovementValueScale.Value = firstActuatorMovementBySound.MovementValueScale;
                    labelValueOffset += $" ({firstActuatorMovementBySound.MovementOffsetMs} ms)";
                    labelValueScale += $" ({firstActuatorMovementBySound.MovementValueScale}%)";
                }
            }

            LabelMovementOffsetMs.Content = labelValueOffset;
            LabelMovementValueScalePercent.Content = labelValueScale;
            _isUpdatingView = false;
        }

        private void EnableEditorControls(bool servoChoiceEnabled, bool propertiesEnabled)
        {
            if (servoChoiceEnabled == true)
            {
                ComboBoxServoToMove.IsEnabled = true;
            } else
            {
                ComboBoxServoToMove.SelectedIndex = 0;
                ComboBoxServoToMove.IsEnabled = false;
            }

            CheckBoxInvertMovement.IsEnabled = propertiesEnabled;
            if (propertiesEnabled == false)
            {
                CheckBoxInvertMovement.IsChecked = false;
            }

            SliderMovementOffsetMs.IsEnabled = propertiesEnabled;
            SliderMovementValueScale.IsEnabled = propertiesEnabled;
        }

        private void ComboBoxSoundToPlay_SelectionChanged(object sender, SelectionChangedEventArgs e) => ValueChoiceChanged();
        private void CheckBoxInvertMovement_Checked(object sender, RoutedEventArgs e) => ValueChoiceChanged();
        private void ComboBoxServoToMove_SelectionChanged(object sender, SelectionChangedEventArgs e) => ValueChoiceChanged();
        private void SliderMovementOffsetMs_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => ValueChoiceChanged();
        private void SliderMovementScale_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => ValueChoiceChanged();

    }
}

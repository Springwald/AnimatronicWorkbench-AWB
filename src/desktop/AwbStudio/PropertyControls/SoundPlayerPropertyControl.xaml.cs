// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Actuators;
using Awb.Core.ActuatorsAndObjects;
using Awb.Core.Player;
using Awb.Core.Project.Servos;
using Awb.Core.Project.Various;
using Awb.Core.Sounds;
using Awb.Core.Timelines;
using AwbStudio.TimelineControls;
using AwbStudio.TimelineEditing;
using System;
using System.Windows;
using System.Windows.Controls;
using Windows.ApplicationModel.Background;

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

        private void ComboBoxSoundToPlay_SelectionChanged(object sender, SelectionChangedEventArgs e) => ValueChoiceChanged();
        private void CheckBoxInvertMovement_Checked(object sender, RoutedEventArgs e) => ValueChoiceChanged();
        private void ComboBoxServoToMove_SelectionChanged(object sender, SelectionChangedEventArgs e) => ValueChoiceChanged();

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
            if (index == 0)
            {
                SetNewSoundValue(null, null,false);
            }
            else
            {
                if (_projectSounds.Length < index)
                {
                    MessageBox.Show("Sound index " + index + " not found!");
                    return;
                }
                var newSound = _projectSounds[index - 1];
                SetNewSoundValue(newSound, movementServoId, movementInverted);
            }
        }

        private void SetNewSoundValue(Sound? sound, string? movementServoId, bool movementInverted)
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

            if (_soundPlayer.ActualMovementServoId != movementServoId || _soundPlayer.ActualMovementInverted != movementInverted)
            {
                if (sound == null)
                {
                    _soundPlayer.SetMovement(null, false);
                }
                else
                {
                    _soundPlayer.SetMovement(movementServoId, movementInverted);
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

            if (soundId == null)
            {
                ComboBoxSoundToPlay.SelectedIndex = 0;

                ComboBoxServoToMove.IsEnabled = false;
                ComboBoxServoToMove.SelectedIndex = 0;

                CheckBoxInvertMovement.IsEnabled = false;
                CheckBoxInvertMovement.IsChecked = false;
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

                string? actualMovementServoTitle = null;
                if (_soundPlayer.ActualMovementServoId != null)
                {
                    foreach (var servo in _servos)
                    {
                        if (servo.Id == _soundPlayer.ActualMovementServoId)
                        {
                            actualMovementServoTitle = servo.Title;
                            break;
                        }
                    }
                }
                ComboBoxServoToMove.SelectedItem = actualMovementServoTitle ?? NoServoTitle;

                CheckBoxInvertMovement.IsEnabled = true;
                CheckBoxInvertMovement.IsChecked = _soundPlayer.ActualMovementInverted;
            }

            _isUpdatingView = false;
        }


    }
}

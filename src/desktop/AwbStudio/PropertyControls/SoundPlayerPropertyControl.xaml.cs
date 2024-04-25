// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Actuators;
using Awb.Core.ActuatorsAndObjects;
using Awb.Core.Player;
using Awb.Core.Project;
using Awb.Core.Sounds;
using Awb.Core.Timelines;
using AwbStudio.TimelineEditing;
using System.Windows;
using System.Windows.Controls;

namespace AwbStudio.PropertyControls
{
    /// <summary>
    /// Interaction logic for SoundPlayerPropertyControl.xaml
    /// </summary>
    public partial class SoundPlayerPropertyControl : UserControl, IPropertyEditor
    {
        private readonly ISoundPlayer _soundPlayer;
        private readonly TimelineData _timelineData;
        private readonly Sound[] _projectSounds;
        private readonly TimelineViewContext _viewContext;
        private readonly PlayPosSynchronizer _playPosSynchronizer;
        private bool _isUpdatingView;

        public IAwbObject AwbObject => _soundPlayer;

        public SoundPlayerPropertyControl(ISoundPlayer soundPlayer, Sound[] projectSounds, TimelineData timelineData, TimelineViewContext viewContext, PlayPosSynchronizer playPosSynchronizer)
        {
            InitializeComponent();
            _soundPlayer = soundPlayer;
            _projectSounds = projectSounds;

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

            ComboBoxSoundToPlay.Items.Add("-- NO SOUND --");
            foreach (var sound in _projectSounds)
            {

                ComboBoxSoundToPlay.Items.Add(sound.Title);
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
            if (e.ChangedObjectId == _soundPlayer.Id) ShowActualValue();
        }

        private void OnPlayPosChanged(object? sender, int e)
        {
            if (_viewContext.ActualFocusObject == _soundPlayer)
                ShowActualValue();
        }

        private void ComboBoxSoundToPlay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdatingView) return;

            var index = ComboBoxSoundToPlay.SelectedIndex;
            if (index == 0)
            {
                SetNewValue(null);
            }
            else
            {
                if (_projectSounds.Length < index)
                {
                    MessageBox.Show("Sound index " + index + " not found!");
                    return;
                }
                var newSoundId = _projectSounds[index-1];
                SetNewValue(newSoundId);
            }
        }

        private void SetNewValue(Sound? sound)
        {
            if (_soundPlayer.ActualSoundId != sound?.Id)
            {
                if (sound == null)
                {
                    _soundPlayer.SetNoSound();
                }
                else
                {
                    _soundPlayer.PlaySound(sound!.Id);
                }
                _viewContext.FocusObjectValueChanged(this);
            }
            ShowActualValue();
        }

        private void ShowActualValue()
        {
            var soundId = _soundPlayer.ActualSoundId;

            _isUpdatingView = true;

            if (soundId == null)
            {
                ComboBoxSoundToPlay.SelectedIndex = 0;
            }
            else
            {
                for (int index = 0; index < _projectSounds.Length; index++)
                {
                    if (_projectSounds[index].Id == soundId)
                    {
                        ComboBoxSoundToPlay.SelectedIndex = index+1;
                        break; ;
                    }
                }
            }

            _isUpdatingView = false;
        }
    }
}

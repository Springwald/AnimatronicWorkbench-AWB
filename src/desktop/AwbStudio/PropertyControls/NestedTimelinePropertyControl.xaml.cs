// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Actuators;
using Awb.Core.ActuatorsAndObjects;
using Awb.Core.Sounds;
using AwbStudio.FileManagement;
using AwbStudio.TimelineEditing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AwbStudio.PropertyControls
{
    /// <summary>
    /// Interaction logic for NestedTimelinePropertyControl.xaml
    /// </summary>
    public partial class NestedTimelinePropertyControl : UserControl, IPropertyEditor
    {
        private readonly ISoundPlayer _soundPlayer;
        private readonly Sound[] _projectSounds;
        private bool _isSetting;
        private int _soundId;
        private TimelineFileManager? _filenameManager;
        private IEnumerable<string> _filenames;

        public IAwbObject AwbObject => _soundPlayer;

        public FileManagement.TimelineFileManager? FileManager
        {
            set
            {
                _filenameManager = value;
                Refresh();
            }
        }

     

        public NestedTimelinePropertyControl()
        {
            InitializeComponent();
            Loaded += SoundPlayerPropertyControl_Loaded;
        }

        private void SoundPlayerPropertyControl_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= SoundPlayerPropertyControl_Loaded;
            foreach (var sound in _projectSounds)
            {
                ComboBoxTimeline.Items.Add(sound.Title);
            }
        }

        public event EventHandler OnValueChanged;

        private void Refresh()
        {
            this.ComboBoxTimeline.Items.Clear();
            if (_filenameManager != null)
            {
                _filenames = _filenameManager.TimelineFilenames;
                foreach (var timelineFilename in _filenames)
                {
                    var timelineMetaData = _filenameManager.GetTimelineMetaData(timelineFilename);
                    if (timelineMetaData == null) continue;
                    this.ComboBoxTimeline.Items.Add($"[{timelineMetaData.StateName}] {timelineMetaData.Title}");
                }
            }
        }


        public async Task UpdateValue(int timeMs)
        {
            SoundId = _soundPlayer.ActualSoundId;
            //if (_projectSounds?.Any() == true)
            //{

            //var soundPoint = _timelineData?.SoundPoints.OfType<SoundPoint>().SingleOrDefault(p => p.SoundPlayerId == soundPlayer.Id && (int)p.TimeMs == _playPosSynchronizer.PlayPosMs); // check existing point
            //if (soundPoint == null)
            //{
            //    // Insert a new sound point
            //    var soundId = soundPlayer.ActualSoundId == 0 ? _projectSounds.FirstOrDefault()?.Id : soundPlayer.ActualSoundId;
            //    var sound = _projectSounds.FirstOrDefault(s => s.Id == soundId);
            //    if (sound == null)
            //    {
            //        MessageBox.Show($"Actual sound id{soundPlayer.ActualSoundId} not found");
            //    }
            //    else
            //    {
            //        soundPoint = new SoundPoint(timeMs: _playPosSynchronizer.PlayPosMs, soundPlayerId: soundPlayer.Id, title: sound.Title, soundId: soundPlayer.ActualSoundId);
            //        _timelineData?.SoundPoints.Add(soundPoint);
            //    }
            //}
            //else
            //{
            //    // Remove the existing sound point
            //    _timelineData?.SoundPoints.Remove(soundPoint);
            //}
            //_timelineData!.SetContentChanged(TimelineDataChangedEventArgs.ChangeTypes.SoundPointChanged, soundPlayer.Id);
            // }
        }


        private void ComboBoxTimeline_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            var index = ComboBoxTimeline.SelectedIndex;
            if (index < 0) return;
            if (_projectSounds.Length <= index)
            {
                MessageBox.Show("Sound index " + index + " not found!");
                return;
            }

            if (_isSetting) return;
            if (_soundPlayer.ActualSoundId == _projectSounds[index].Id) return;

            SoundId = _projectSounds[index].Id;
            _soundPlayer.PlaySound(_projectSounds[index].Id);
            OnValueChanged?.Invoke(this, new EventArgs());
        }

        private int SoundId
        {
            get => _soundId;
            set
            {
                if (value.Equals(_soundId)) return;
                _soundId = value;
                _isSetting = true;
                ComboBoxTimeline.SelectedIndex = Array.FindIndex(_projectSounds, s => s.Id == value);
                _isSetting = false;
            }
        }
    }
}


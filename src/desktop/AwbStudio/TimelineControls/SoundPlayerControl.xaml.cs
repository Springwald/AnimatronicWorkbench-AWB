// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License


using Awb.Core.Player;
using Awb.Core.Sounds;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Diagnostics;
using AwbStudio.Tools;

namespace AwbStudio.TimelineControls
{
    /// <summary>
    /// Interaction logic for SoundPlayerControl.xaml
    /// </summary>
    public partial class SoundPlayerControl : UserControl
    {
        private int _lastPlayedSoundId;
        private DateTime _lastPlayedSoundUtcTime;
        private MediaPlayer? _mediaPlayer;

        public Sound[]? Sounds { get; internal set; }

        public SoundPlayerControl()
        {
            InitializeComponent();
            Loaded += SoundPlayerControl_Loaded;
        }

        private void SoundPlayerControl_Loaded(object sender, RoutedEventArgs e)
        {
            _mediaPlayer = new MediaPlayer();
            Unloaded += SoundPlayerControl_Unloaded;
        }

        private void SoundPlayerControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Unloaded -= SoundPlayerControl_Unloaded;
            if (_mediaPlayer != null)
            {
                _mediaPlayer.Stop();
                _mediaPlayer = null;
            }
        }

        public void SoundToPlay(object? sender, SoundPlayEventArgs e)
        {
            if (_mediaPlayer == null) return;
            if (Sounds == null) return;
            var sound = Sounds.FirstOrDefault(s => s.Id == e.SoundId);
            if (sound == null)
            {
                MessageBox.Show($"Sound id '{e.SoundId}' not found.");
            }
            else
            {
                Debug.WriteLine((DateTime.UtcNow - _lastPlayedSoundUtcTime).TotalSeconds);
                if (_lastPlayedSoundId == sound.Id && (DateTime.UtcNow - _lastPlayedSoundUtcTime).TotalSeconds < 1)
                {
                    // do not spam with sound playing...
                }
                else
                {
                    _lastPlayedSoundId = sound.Id;
                    _lastPlayedSoundUtcTime = DateTime.UtcNow;
                    MyInvoker.Invoke(new Action(() =>
                    {
                        _mediaPlayer.Stop();
                        _mediaPlayer.Open(new Uri(sound.Mp3Filename));
                        _mediaPlayer.Play();
                    }));
                }
            }
        }
    }
}

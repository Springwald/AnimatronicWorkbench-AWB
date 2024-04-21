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
using System.Windows;
using System.Windows.Controls;

namespace AwbStudio.PropertyControls
{
    /// <summary>
    /// Interaction logic for NestedTimelinePropertyControl.xaml
    /// </summary>
    public partial class NestedTimelinePropertyControl : UserControl, IPropertyEditor
    {
        private TimelineFileManager? _filenameManager;
        private IEnumerable<string> _filenames;

        public IAwbObject AwbObject => NestedTimelinesFakeObject.Singleton;

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
    }
}


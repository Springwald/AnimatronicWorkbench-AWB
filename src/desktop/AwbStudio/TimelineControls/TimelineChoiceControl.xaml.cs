// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using AwbStudio.FileManagement;
using AwbStudio.TimelineEditing;
using System;
using System.Windows.Controls;

namespace AwbStudio.TimelineControls
{
    /// <summary>
    /// Interaction logic for TimelineChoiceControl.xaml
    /// </summary>
    public partial class TimelineChoiceControl : UserControl
    {
        private FileManager? _filenameManager;

        public EventHandler<TimelineNameChosenEventArgs>? OnTimelineChosen;

        public FileManager? FileManager
        {
            set
            {
                _filenameManager = value;
                Refresh();
            }
        }

        public TimelineChoiceControl()
        {
            InitializeComponent();
        }

        public void Refresh()
        {
            this.PanelNames.Children.Clear();
            if (_filenameManager == null)
            {
                this.LabelTitle.Content = $"Timeline Manager";
            }
            else
            {
                this.LabelTitle.Content = $"{_filenameManager.ProjectTitle} Timelines";
                foreach (var timelineFilename in _filenameManager.TimelineFilenames)
                {
                    var timelineMetaData = _filenameManager.GetTimelineMetaData(timelineFilename);
                    if (timelineMetaData == null) continue;
                    var button = new Button { Content = $"[{timelineMetaData.StateName}] {timelineMetaData.Title}", Tag = timelineFilename };
                    button.Click += (s, e) => { OnTimelineChosen?.Invoke(this, new TimelineNameChosenEventArgs(timelineFilename)); };
                    this.PanelNames.Children.Add(button);
                }
            }
        }
    }
}

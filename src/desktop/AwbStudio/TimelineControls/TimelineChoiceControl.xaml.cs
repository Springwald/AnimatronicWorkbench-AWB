// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Services;
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
        private ITimelineDataService? _timelineDataService;

        public EventHandler<TimelineNameChosenEventArgs>? OnTimelineChosen;
        private string _projectTitle;

        public ITimelineDataService? FileManager
        {
            set
            {
                _timelineDataService = value;
                Refresh();
            }
        }

        public string ProjectTitle
        {
            set
            {
                _projectTitle = value;
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
            if (_timelineDataService == null)
            {
                this.LabelTitle.Content = $"Timeline Manager";
            }
            else
            {
                this.LabelTitle.Content = $"{_projectTitle ?? "No project title"} Timelines";
                foreach (var timelineId in _timelineDataService.TimelineIds)
                {
                    var timelineMetaData = _timelineDataService.GetTimelineData(timelineId);
                    if (timelineMetaData == null) continue;
                    var stateName = timelineMetaData.TimelineStateId; // todo: get state name from the project  states instead
                    var button = new Button { Content = $"[{stateName}] {timelineMetaData.Title}", Tag = timelineId };
                    button.Click += (s, e) => { OnTimelineChosen?.Invoke(this, new TimelineNameChosenEventArgs( timelineId: timelineId)); };
                    this.PanelNames.Children.Add(button);
                }
            }
        }
    }
}

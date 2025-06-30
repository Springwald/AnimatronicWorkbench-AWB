// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Services;
using Awb.Core.Timelines;
using AwbStudio.TimelineEditing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace AwbStudio.TimelineControls
{
    public partial class TimelineChoiceControl : UserControl
    {
        private ITimelineDataService? _timelineDataService;

        public EventHandler<TimelineNameChosenEventArgs>? OnTimelineChosen;
        private string? _projectTitle;

        public ITimelineDataService? FileManager
        {
            set
            {
                _timelineDataService = value;
                Refresh();
            }
        }

        public required string? ProjectTitle
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
                var timelines = new List<TimelineData>();
                foreach (var timelineId in _timelineDataService.TimelineIds)
                {
                    var timelineMetaData = _timelineDataService.GetTimelineData(timelineId);
                    if (timelineMetaData == null) continue;
                    timelines.Add(timelineMetaData);
                }
                foreach (var timelineMetaData in timelines.OrderBy(t => t.TimelineStateId).ThenBy(t => t.Title))
                {
                    var stateName = timelineMetaData.TimelineStateId; // todo: get state name from the project  states instead
                    var button = new Button { Content = $"[{stateName}] {timelineMetaData.Title}", Tag = timelineMetaData.Id };
                    button.Click += (s, e) => { OnTimelineChosen?.Invoke(this, new TimelineNameChosenEventArgs(timelineId: timelineMetaData.Id)); };
                    this.PanelNames.Children.Add(button);
                }
            }
        }
    }
}

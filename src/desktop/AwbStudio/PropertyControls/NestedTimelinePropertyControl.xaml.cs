﻿// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.ActuatorsAndObjects;
using Awb.Core.Player;
using Awb.Core.Project;
using Awb.Core.Timelines;
using AwbStudio.TimelineEditing;
using System.Windows;
using System.Windows.Controls;

namespace AwbStudio.PropertyControls
{
    /// <summary>
    /// Interaction logic for NestedTimelinePropertyControl.xaml
    /// </summary>
    public partial class NestedTimelinePropertyControl : UserControl, IPropertyEditor
    {
        private readonly ITimelineMetaDataService _timelineMetaDataService;
        private readonly TimelineData _timelineData;
        private readonly TimelineViewContext _viewContext;
        private readonly PlayPosSynchronizer _playPosSynchronizer;
        private bool _isUpdatingView;

        public IAwbObject AwbObject => NestedTimelinesFakeObject.Singleton;


        public NestedTimelinePropertyControl(ITimelineMetaDataService timelineMetaDataService, TimelineData timelineData, TimelineViewContext viewContext, PlayPosSynchronizer playPosSynchronizer)
        {
            InitializeComponent();
            Loaded += NestedTimelinePropertyControl_Loaded;
            _timelineMetaDataService = timelineMetaDataService;
            _timelineData = timelineData;
            _viewContext = viewContext;
            _playPosSynchronizer = playPosSynchronizer;
            
        }

        private void NestedTimelinePropertyControl_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= NestedTimelinePropertyControl_Loaded;
            Unloaded += NestedTimelinePropertyControl_Unloaded;
            _timelineData.OnContentChanged += TimelineData_OnContentChanged;
            _viewContext.Changed += ViewContext_Changed;
            _playPosSynchronizer.OnPlayPosChanged += PlayPosSynchronizer_OnPlayPosChanged;
            RefreshList();
        }

        private void NestedTimelinePropertyControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Unloaded -= NestedTimelinePropertyControl_Unloaded;
            _viewContext.Changed -= ViewContext_Changed;
            _playPosSynchronizer.OnPlayPosChanged -= PlayPosSynchronizer_OnPlayPosChanged;
            _timelineData.OnContentChanged -= TimelineData_OnContentChanged;
        }

        private void TimelineData_OnContentChanged(object? sender, TimelineDataChangedEventArgs e)
        {
            if (e.ChangedObjectId == NestedTimelinesFakeObject.Singleton.Id) ShowActualValue();
        }

        private void ViewContext_Changed(object? sender, ViewContextChangedEventArgs e)
        {
            switch (e.ChangeType)
            {
                case ViewContextChangedEventArgs.ChangeTypes.FocusObject:
                case ViewContextChangedEventArgs.ChangeTypes.FocusObjectValue:
                    if (_viewContext.ActualFocusObject == NestedTimelinesFakeObject.Singleton)
                        ShowActualValue();
                    break;
            }
        }

        private void PlayPosSynchronizer_OnPlayPosChanged(object? sender, int e)
        {
            if (_viewContext.ActualFocusObject == NestedTimelinesFakeObject.Singleton)
                ShowActualValue();
        }

        private void ComboBoxTimeline_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdatingView) return;

            var index = ComboBoxTimeline.SelectedIndex;
            if (index == 0)
            {
                SetNewValue(null);
            }
            else
            {
                var timeslines = _timelineMetaDataService.GetAllMetaData();
                if (timeslines.Length < index)
                {
                    MessageBox.Show("Timelines index " + index + " not found!");
                    return;
                }
                var newTimelineId = timeslines[index - 1].Id;
                SetNewValue(newTimelineId);
            }
        }

        private void SetNewValue(string? timelineId)
        {
            if (NestedTimelinesFakeObject.Singleton.ActualNestedTimelineId != timelineId)
            {
                NestedTimelinesFakeObject.Singleton.ActualNestedTimelineId = timelineId;
                _viewContext.FocusObjectValueChanged(this);
            }
            ShowActualValue();
        }

        private void ShowActualValue()
        {
            var timelineId = NestedTimelinesFakeObject.Singleton.ActualNestedTimelineId;

            _isUpdatingView = true;

            if (timelineId == null)
            {
                ComboBoxTimeline.SelectedIndex = 0;
            }
            else
            {
                for (int index = 0; index < _timelineMetaDataService.GetAllMetaData().Length; index++)
                {
                    if (_timelineMetaDataService.GetAllMetaData()[index]?.Id == timelineId)
                    {
                        ComboBoxTimeline.SelectedIndex = index + 1;
                        break; ;
                    }
                }
            }
            _isUpdatingView = false;
        }

        private void RefreshList()
        {
            this.ComboBoxTimeline.Items.Clear();
            var timelineMetaDatas = _timelineMetaDataService.GetAllMetaData();
            ComboBoxTimeline.Items.Add("-- NO TIMELINE --");
            foreach (var timelineMetaData in timelineMetaDatas)
            {
                if (timelineMetaData == null)
                {
                    this.ComboBoxTimeline.Items.Add($"TimeLineMetaData == null ?!?");
                }
                else
                {
                    var stateName = timelineMetaData.StateId; // todo: get state name from the project config
                    this.ComboBoxTimeline.Items.Add($"[{stateName}] {timelineMetaData.Title}");
                }
            }
        }

    }
}


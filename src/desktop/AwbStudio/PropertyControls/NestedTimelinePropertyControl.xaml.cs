// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.ActuatorsAndObjects;
using AwbStudio.FileManagement;
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
        private readonly TimelineViewContext _viewContext;
        private bool _isUpdatingView;

        public IAwbObject AwbObject => NestedTimelinesFakeObject.Singleton;


        public NestedTimelinePropertyControl(ITimelineMetaDataService timelineMetaDataService, TimelineViewContext viewContext)
        {
            InitializeComponent();
            Loaded += NestedTimelinePropertyControl_Loaded;
            _timelineMetaDataService = timelineMetaDataService;
            _viewContext = viewContext;
        }

        private void NestedTimelinePropertyControl_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= NestedTimelinePropertyControl_Loaded;
            RefreshList();
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
                if (timeslines.Length <= index)
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
                    this.ComboBoxTimeline.Items.Add($"[{timelineMetaData.StateName}] {timelineMetaData.Title}");
                }
            }
        }

    }
}


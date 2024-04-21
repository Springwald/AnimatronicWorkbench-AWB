// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.ActuatorsAndObjects;
using AwbStudio.FileManagement;
using AwbStudio.TimelineEditing;
using System;
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

        public IAwbObject AwbObject => NestedTimelinesFakeObject.Singleton;


        public NestedTimelinePropertyControl(ITimelineMetaDataService timelineMetaDataService)
        {
            InitializeComponent();
            Loaded += NestedTimelinePropertyControl_Loaded;
            _timelineMetaDataService = timelineMetaDataService;
        }

        private void NestedTimelinePropertyControl_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= NestedTimelinePropertyControl_Loaded;
            RefreshList();
        }

        public event EventHandler OnValueChanged;

        private void RefreshList()
        {
            this.ComboBoxTimeline.Items.Clear();
            var timelineMetaDatas = _timelineMetaDataService.GetAllMetaData();
            foreach (var timelineMetaData in timelineMetaDatas)
            {
                if (timelineMetaData == null) continue;
                this.ComboBoxTimeline.Items.Add($"[{timelineMetaData.StateName}] {timelineMetaData.Title}");
            }
        }

      
    }
}


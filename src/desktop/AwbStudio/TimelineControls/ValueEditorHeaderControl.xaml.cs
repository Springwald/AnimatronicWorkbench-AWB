// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using System.Windows.Controls;
using AwbStudio.TimelineEditing;

namespace AwbStudio.TimelineControls
{
    /// <summary>
    /// Interaction logic for ValueEditorHeaderControl.xaml
    /// </summary>
    public partial class ValueEditorHeaderControl : UserControl
    {
        private TimelineCaption? _timelineCaption;
        private TimelineViewContext? _viewContext;

        public ValueEditorHeaderControl()
        {
            InitializeComponent();
        }

        public TimelineViewContext ViewContext
        {
             set
            {
                this._viewContext = value;
                this._viewContext.Changed +=(sender, e) =>
                {
                    this.LabelTitle.Content = _timelineCaption?.Label;
                };
            }
        }

        public TimelineCaption? TimelineCaption
        {
            get => _timelineCaption;
            set
            {
                _timelineCaption = value;
                this.LabelTitle.Content = value?.Label;
            }
        }
    }
}

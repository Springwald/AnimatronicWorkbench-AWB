// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.ActuatorsAndObjects;
using AwbStudio.TimelineEditing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AwbStudio.TimelineControls
{
    /// <summary>
    /// Interaction logic for ValueEditorHeaderControl.xaml
    /// </summary>
    public partial class ValueEditorHeaderControl : UserControl
    {
        private TimelineCaption? _timelineCaption;
        private TimelineViewContext? _viewContext;
        private Brush? _backupBackground;

        public ValueEditorHeaderControl()
        {
            InitializeComponent();
            Loaded += ValueEditorHeaderControl_Loaded;
        }

        private void ValueEditorHeaderControl_Loaded(object sender, RoutedEventArgs e)
        {
            _backupBackground = this.Background;
        }

        public IAwbObject? MyObject { get; set; }

        public TimelineViewContext ViewContext
        {
            set
            {
                this._viewContext = value;
                this._viewContext.Changed += (sender, e) =>
                {
                    this.LabelTitle.Content = _timelineCaption?.Label;
                    if (_viewContext.ActualFocusObject == MyObject)
                    {
                        this.Background = System.Windows.Media.Brushes.DarkGray;
                        this.LabelTitle.FontWeight = FontWeights.Bold;
                        this.LabelTitle.Foreground = System.Windows.Media.Brushes.Black;
                    }
                    else
                    {
                        this.Background = _backupBackground;
                        this.LabelTitle.FontWeight = FontWeights.Normal;
                        this.LabelTitle.Foreground = System.Windows.Media.Brushes.White;
                    }
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

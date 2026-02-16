// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2026 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using AwbStudio.TimelineEditing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AwbStudio.TimelineControls
{
    public partial class ValueEditorSelectionContainerControl : UserControl
    {
        private TimelineCaption? _timelineCaption;
        private TimelineViewContext? _viewContext;
        private Brush? _backupBackground;

        public ValueEditorSelectionContainerControl()
        {
            InitializeComponent();
            Loaded += ValueEditorHeaderControl_Loaded;
        }

        private void ValueEditorHeaderControl_Loaded(object sender, RoutedEventArgs e)
        {
            _backupBackground = this.Background;
        }

        /// <summary>
        /// The object for which the values are edited in this container
        /// </summary>
        public IAwbObjectControl? MyObjectToEdit { get; private set; }

        /// <summary>
        ///  The editor control that is shown in this container to edit the values of MyObject. 
        /// </summary>
        public UserControl? MyEditorControl { get; private set; }

        public void Init(IAwbObjectControl? objectToEdit, UserControl editorControl, TimelineViewContext viewContext, TimelineCaption timelineCaption)
        {
            MyObjectToEdit = objectToEdit;
            MyEditorControl = editorControl;

            this._timelineCaption = timelineCaption;
            this._viewContext = viewContext;
            this._viewContext.Changed += (sender, e) =>
            {
                this.LabelTitle.Content = _timelineCaption?.Label;
                if (_viewContext.ActualFocusObject == MyObjectToEdit)
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

            LabelTitle.Content = _timelineCaption?.Label;
            StackPanelEditorControls.Children.Add(editorControl);
        }
    }
}

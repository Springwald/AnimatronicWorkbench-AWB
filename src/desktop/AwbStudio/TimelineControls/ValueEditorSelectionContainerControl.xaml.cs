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
                if (_viewContext.ActualFocusObject == MyObjectToEdit?.AwbObject)
                {
                    // my object is the actual selected object
                    this.Background = new SolidColorBrush(Color.FromArgb(40, 100, 100,255));
                    this.LabelTitle.FontWeight = FontWeights.Bold;
                    this.LabelTitle.Foreground = System.Windows.Media.Brushes.White;
                }
                else
                {
                    // my object is not the actual selected object
                    this.Background = _backupBackground;
                    this.LabelTitle.FontWeight = FontWeights.Normal;
                    this.LabelTitle.Foreground = System.Windows.Media.Brushes.Gray;
                }
            };

            LabelTitle.Content = _timelineCaption?.Label;
            StackPanelEditorControls.Children.Add(editorControl);
        }

        /// <summary>
        /// The values editor container is focused when the user clicks on it. 
        /// </summary>
        private void StackPanelEditorControls_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_viewContext == null || MyObjectToEdit == null) return;

            // report the focus change to the timeline view context, so that other controls can react to it.
            _viewContext.ActualFocusObject = MyObjectToEdit.AwbObject;
        }
    }
}

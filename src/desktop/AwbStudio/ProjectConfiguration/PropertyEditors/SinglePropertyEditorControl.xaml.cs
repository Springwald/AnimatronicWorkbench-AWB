// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using System.Windows;
using System.Windows.Controls;

namespace AwbStudio.ProjectConfiguration.PropertyEditors
{
    /// <summary>
    /// Interaction logic for SinglePropertyEditorControl.xaml
    /// </summary>
    public partial class SinglePropertyEditorControl : UserControl
    {
        /// <summary>
        /// The name of the property to edit
        /// </summary>
        public string PropertyName
        {
            get { return (string)GetValue(PropertyNameProperty); }
            set { SetValue(PropertyNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PropertyName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PropertyNameProperty =
            DependencyProperty.Register("PropertyName", typeof(string), typeof(SinglePropertyEditorControl), new PropertyMetadata(null));


        /// <summary>
        /// the content of the property
        /// </summary>
        public string PropertyContent
        {
            get { return (string)GetValue(PropertyContentProperty); }
            set { SetValue(PropertyContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PropertyContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PropertyContentProperty =
            DependencyProperty.Register("PropertyContent", typeof(string), typeof(SinglePropertyEditorControl), new PropertyMetadata(null));


        /// <summary>
        /// the regular expression pattern for validation
        /// </summary>
        public string ValidationPattern
        {
            get { return (string)GetValue(ValidationPatternProperty); }
            set { SetValue(ValidationPatternProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ValidationPattern.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValidationPatternProperty =
            DependencyProperty.Register("ValidationPattern", typeof(string), typeof(SinglePropertyEditorControl), new PropertyMetadata(null));

        public SinglePropertyEditorControl()
        {
            InitializeComponent();
        }
    }
}

// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Actuators;
using Awb.Core.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AwbStudio.ProjectConfiguration.PropertyEditors
{
    /// <summary>
    /// Interaction logic for ScsServoEditorControl.xaml
    /// </summary>
    public partial class ScsServoEditorControl : UserControl
    {

        public static readonly DependencyProperty StsServoConfigProperty =
            DependencyProperty.Register(nameof(StsServoConfig),typeof(StsServoConfig),typeof(ScsServoEditorControl),
                new PropertyMetadata(null)
                               //new PropertyMetadata("DEFAULT")
                                               );

        public StsServoConfig StsServoConfig
        {
            get { return (StsServoConfig)GetValue(StsServoConfigProperty); }
            set { 
                SetValue(StsServoConfigProperty, value); 
            }
        }

        public IEnumerable<string> Errors { get; private set; }

        public ScsServoEditorControl()
        {
            InitializeComponent();
        }

        private void WriteDataToEditor(StsServoConfig stsServoConfig)
        {

        }

        private void ReadDataFromEditor(StsServoConfig stsServoConfig)
        {

        }

        private SinglePropertyEditorControl AddEditor(string name, string value, string validationPattern)
        {
            var editor = new SinglePropertyEditorControl();
            editor.PropertyName = name;
            editor.PropertyContent = value;
            editor.ValidationPattern = validationPattern;
            this.EditorStackPanel.Children.Add(editor);
            return editor;
        }
    }
}

// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Project;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace AwbStudio.ProjectConfiguration.PropertyEditors
{
    /// <summary>
    /// Interaction logic for ScsServoEditorControl.xaml
    /// </summary>
    public partial class ScsServoEditorControl : UserControl
    {

        private List<ValueEditorControl> _editors;

        public static readonly DependencyProperty StsServoConfigProperty =
            DependencyProperty.Register(nameof(StsServoConfig), typeof(StsServoConfig), typeof(ScsServoEditorControl),
                new PropertyMetadata(null)
                                               //new PropertyMetadata("DEFAULT")
                                               );

        public StsServoConfig StsServoConfig
        {
            get
            {
                var value = (StsServoConfig)GetValue(StsServoConfigProperty);
                ReadDataFromEditor(value);
                return value;
            }
            set
            {
                
                SetValue(StsServoConfigProperty, value);
                WriteDataToEditor(value);
            }
        }

        public IEnumerable<string> Errors { get; private set; }

        public ScsServoEditorControl()
        {
            InitializeComponent();
            this.DataContext = StsServoConfig;
        }


        private void WriteDataToEditor(StsServoConfig stsServoConfig)
        {
            if (_editors != null) throw new System.Exception("Object to edit is already set.");
            var type = stsServoConfig.GetType();
            _editors = new List<ValueEditorControl>();
            /*foreach (var property in type.GetProperties())
            {
                var editor = new ValueEditorControl();
                editor.SetPropertyToEdit(() => stsServoConfig.GetType().GetProperty(property.Name));
                _editors.Add(editor);
                this.EditorStackPanel.Children.Add(editor);
            }*/

            var editor = new ValueEditorControl();
            editor.SetPropertyToEdit(() => stsServoConfig.ClientId);
            _editors.Add(editor);
            this.EditorStackPanel.Children.Add(editor);

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

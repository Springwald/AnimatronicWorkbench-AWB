// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Project;
using Awb.Core.Project.Servos;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
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

        public static readonly DependencyProperty ScsServoConfigProperty =
            DependencyProperty.Register(nameof(ScsFeetechServoConfig), typeof(ScsFeetechServoConfig), typeof(ScsServoEditorControl),
                new PropertyMetadata(null));

        public ScsFeetechServoConfig ScsServoConfig
        {
            get
            {
                var value = (ScsFeetechServoConfig)GetValue(ScsServoConfigProperty);
                ReadDataFromEditor(value);
                return value;
            }
            set
            {
                
                SetValue(ScsServoConfigProperty, value);
                WriteDataToEditor(value);
            }
        }

        public IEnumerable<string> Errors { get; private set; }

        public ScsServoEditorControl()
        {
            InitializeComponent();
            this.DataContext = ScsServoConfig;
        }

        private void WriteDataToEditor(IProjectObjectListable stsServoConfig)
        {
            if (_editors != null) throw new System.Exception("Object to edit is already set.");

            

            var type = stsServoConfig.GetType();
            _editors = new List<ValueEditorControl>();

            // iterate trough all properties  of the object which have a DataAnnotation for "Name" and add an editor for each
            foreach (var property in type.GetProperties())
            {
                // check if the property has a name set via DisplayName Annotation
                var displayNameAttribute = property.GetCustomAttribute<DisplayNameAttribute>();
                if (displayNameAttribute == null) continue;

                var editor = new ValueEditorControl();
                //editor.SetPropertyToEditByExpression(() => stsServoConfig.GetType().GetProperty(property.Name));
                editor.SetPropertyToEditByName(target: stsServoConfig, propertyName: property.Name);
                editor.PropertyChanged += (s, e) =>
                {
                    LabelObjectTypeTitle.Content = stsServoConfig.TitleDetailled;
                };
                _editors.Add(editor);
                this.EditorStackPanel.Children.Add(editor);
            }

            //editor.SetPropertyToEditByExpression(() => stsServoConfig.ClientId);
            //editor.SetPropertyToEditByName(stsServoConfig, "ClientId");
            //_editors.Add(editor);
            //this.EditorStackPanel.Children.Add(editor);

        }

        private void ReadDataFromEditor(ScsFeetechServoConfig scsServoConfig)
        {

        }


    }
}

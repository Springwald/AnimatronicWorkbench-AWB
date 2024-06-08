// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Controls;

namespace AwbStudio.ProjectConfiguration.PropertyEditors
{
    /// <summary>
    /// Interaction logic for ValueEditorControl.xaml
    /// </summary>
    public partial class ValueEditorControl : UserControl
    {
        private object? _targetObject;
        private string? _propertyName;

        public string? PropertyTitle { get; set; }

        public string? ErrorMessagesJoined { get; private set; }

        public string? PropertyContentText
        {
            get;
            set;
        }

        public bool PropertyContentBool
        {
            get;
            set;
        }

        public string? PropertyDescription { get; private set; }

        public ValueEditorControl()
        {
            InitializeComponent();
        }

        /*
        /// <summary>
        /// set property to edit by property expression
        /// </summary>
        /// <example>
        /// SetPropertyToEditByExpression(() => stsServoConfig.ClientId);
        /// </example>
        public void SetPropertyToEditByExpression<T>(Expression<Func<T>> property)
        {
            var propertyInfo = ((MemberExpression)property.Body).Member as PropertyInfo;
            if (propertyInfo == null)
                throw new ArgumentException("The lambda expression 'property' should point to a valid Property");

            PropertyTitle = propertyInfo.Name;
            PropertyContentText = property.Compile()()?.ToString() ?? string.Empty;
        }
        */

        /// <summary>
        /// Set property to edit by name
        /// </summary>
        public void SetPropertyToEditByName(object target, string propertyName)
        {
            _targetObject = target;
            _propertyName = propertyName;

            var prop = target.GetType().GetProperty(propertyName);
            if (prop == null) throw new Exception($"Property '{propertyName}' not found");

            // get the title of the property using the DisplayName annotation
            var displayNameAttribute = prop.GetCustomAttribute<DisplayNameAttribute>();
            PropertyTitle = displayNameAttribute?.DisplayName ?? propertyName;

            // get get description of the property using the Description annotation
            var descriptionAttribute = prop.GetCustomAttribute<DescriptionAttribute>();
            PropertyDescription = descriptionAttribute?.Description ?? string.Empty;

            // activate the correct editor control for this type of property
            // e.g. a checkbox for bool, a textbox for string, a numeric up down for int, etc.
            var value = prop.GetValue(target); // get the value of the property
            

            var propertyType = prop.PropertyType;
            if (propertyType == typeof(bool))
            {
                // for bool properties use the checkbox editor
                PropertyContentBool = value != null && (bool)value;
                TextPropertyContentTextEditor.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                // for all other types use the text editor
                PropertyContentText = value?.ToString() ?? string.Empty;
                CheckBoxPropertyContentBoolEditor.Visibility = System.Windows.Visibility.Collapsed;
            }


            //if (!string.IsNullOrEmpty(input))
            //{
            //    var prop = target.GetType().GetProperty(propertyName);
            //    prop.SetValue(target, input);
            //}
        }

        private void TextBoxPropertyContent_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void CheckBoxPropertyContentBoolEditor_Checked(object sender, System.Windows.RoutedEventArgs e)
        {

        }
    }
}

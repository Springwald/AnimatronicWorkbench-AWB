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
using System.Windows.Documents;

namespace AwbStudio.ProjectConfiguration.PropertyEditors
{
    /// <summary>
    /// Interaction logic for ValueEditorControl.xaml
    /// </summary>
    public partial class ValueEditorControl : UserControl//, INotifyPropertyChanged
    {
        private object _targetObject;
        private string _propertyName;

        public string PropertyTitle { get; set; }

        public string PropertyContent
        { get; set; }
        /*{
            get { return (string)GetValue(PropertyContentProperty); }
            set { SetValue(PropertyContentProperty, value); }
        } 

        public static readonly DependencyProperty PropertyContentProperty =
            DependencyProperty.Register("PropertyContent", typeof(string), typeof(ValueEditorControl), new PropertyMetadata(null));
        */

        public ValueEditorControl()
        {
            InitializeComponent();
        }

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

            PropertyTitle= propertyInfo.Name;
            PropertyContent = property.Compile()()?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Set property to edit by name
        /// </summary>
        public void SetPropertyToEditByName(object target, string propertyName)
        {
            _targetObject = target;
            _propertyName = propertyName;

            var prop = target.GetType().GetProperty(propertyName);
            if (prop == null) throw new Exception($"Property '{propertyName}' not found");

            // get the value of the property
            var value = prop.GetValue(target);
            PropertyContent = value?.ToString() ?? string.Empty;

            // get the title of the property using the DisplayName annotation
            var displayNameAttribute = prop.GetCustomAttribute<DisplayNameAttribute>();
            PropertyTitle = displayNameAttribute?.DisplayName ?? propertyName;

            //if (!string.IsNullOrEmpty(input))
            //{
            //    var prop = target.GetType().GetProperty(propertyName);
            //    prop.SetValue(target, input);
            //}
        }

        //protected virtual void OnPropertyChanged(string propertyName)
        //{
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //}

        //public event PropertyChangedEventHandler PropertyChanged;
    }
}

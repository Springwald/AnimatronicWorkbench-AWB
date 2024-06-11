// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Tools;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace AwbStudio.ProjectConfiguration.PropertyEditors
{
    /// <summary>
    /// Interaction logic for ValueEditorControl.xaml
    /// </summary>
    public partial class ValueEditorControl : UserControl, INotifyPropertyChanged
    {
        private object? _targetObject;
        private string? _propertyName;

        private string? _propertyTitle;
        public string? PropertyTitle
        {
            get { return _propertyTitle; }
            set
            {
                if (_propertyTitle != value)
                {
                    _propertyTitle = value;
                    OnPropertyChanged();
                }
            }
        }

        private string? _errorMessagesJoined;
        public string? ErrorMessagesJoined
        {
            get { return _errorMessagesJoined; }
            set
            {
                if (_errorMessagesJoined != value)
                {
                    _errorMessagesJoined = value;
                    OnPropertyChanged();
                }
            }
        }

        private string? _propertyContentText;
        public string? PropertyContentText
        {
            get { return _propertyContentText; }
            set
            {
                if (_propertyContentText != value)
                {
                    _propertyContentText = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _propertyContentBool;
        public bool PropertyContentBool
        {
            get { return _propertyContentBool; }
            set
            {
                if (_propertyContentBool != value)
                {
                    _propertyContentBool = value;
                    OnPropertyChanged();
                }
            }
        }

        private string? _propertyDescription;
        public string? PropertyDescription
        {
            get { return _propertyDescription; }
            set
            {
                if (_propertyDescription != value)
                {
                    _propertyDescription = value;
                    OnPropertyChanged();
                }
            }
        }

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

            _ = CheckErrorMessages(value);
        }

        private void TextBoxPropertyContent_TextChanged(object sender, TextChangedEventArgs e)
        {
            ErrorMessagesJoined = string.Empty;

            // set the value of the property to the value of the textbox,
            // casting to the correct type e.g. int
            if (_targetObject != null && _propertyName != null)
            {
                var newValue = TextPropertyContentTextEditor.Text;
                if (CheckErrorMessages(newValue) == false) return;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(_propertyName));
            }
        }

        private bool CheckErrorMessages(object? newValue)
        {
            if (_targetObject == null || _propertyName == null) return false;

            var prop = _targetObject.GetType().GetProperty(_propertyName);
            if (prop == null) throw new Exception($"Property '{_propertyName}' not found");

            var attributeErrors = GetErrorMessagesByValidationAttributes(newValue);
            if (!string.IsNullOrWhiteSpace(attributeErrors))
            {
                ErrorMessagesJoined = attributeErrors;
                return false;
            }

            var typeValueErrors = GetErrorMessagesByType(prop, newValue);
            if (!string.IsNullOrWhiteSpace(typeValueErrors))
            {
                // validate the property value using DataAnnotations (e.g. Range, Required, etc.)
                ErrorMessagesJoined = typeValueErrors;
                return false;
            }

            return true;
        }

        private string? GetErrorMessagesByType(PropertyInfo? prop, object? newValue)
        {
            bool isNullable = IsNullable(prop);

            if ((newValue == null) || (newValue is string && string.IsNullOrWhiteSpace(newValue as string)))
            {
                if (isNullable)
                    prop.SetValue(_targetObject, null);
                else
                    return "Value is empty";
            }

            if (newValue is string newStringValue)
            {
                var propertyType = prop.PropertyType;

                if (propertyType == typeof(int) || propertyType == typeof(int?))
                {
                    if (int.TryParse(newStringValue, out var intValue))
                    {
                        prop.SetValue(_targetObject, intValue);
                    }
                    else
                    {
                        return "Value is not a valid integer";
                    }
                }
                else if (propertyType == typeof(uint) || propertyType == typeof(uint?))
                {
                    if (uint.TryParse(newStringValue, out var uintValue))
                    {
                        prop.SetValue(_targetObject, uintValue);
                    }
                    else
                    {
                        return "Value is not a valid unsigned integer";
                    }
                }
                else if (propertyType == typeof(string))
                {
                    prop.SetValue(_targetObject, newValue);
                }
                else
                {
                    throw new Exception($"Unsupported property type '{propertyType}'");
                }
            }

            return null;

        }

        private static bool IsNullable(PropertyInfo? property)
        {
            if (property == null) return false;

            //if (property.PropertyType == typeof(string) &&
            if (property.GetMethod?.CustomAttributes.Any(x => x.AttributeType.Name == "NullableContextAttribute") == true)
                return true;

            // first check if the value is null or empty
            return Nullable.GetUnderlyingType(property.PropertyType) != null;
        }

        private void CheckBoxPropertyContentBoolEditor_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            ErrorMessagesJoined = string.Empty;

            // set the value of the property to the value of the checkbox
            if (_targetObject != null && _propertyName != null)
            {
                var prop = _targetObject.GetType().GetProperty(_propertyName);
                if (prop == null) throw new Exception($"Property '{_propertyName}' not found");
                prop.SetValue(_targetObject, true);
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(_propertyName));
        }

        private string GetErrorMessagesByValidationAttributes(object? value)
        {
            if (_targetObject != null && _propertyName != null)
            {
                var objType = _targetObject.GetType();
                var prop = objType.GetProperty(_propertyName);
                if (prop == null) throw new Exception($"Property '{_propertyName}' not found");

                var errors = NonGenericValidator.ValidateProperty(objType, value, _propertyName);
                return string.Join("; ", errors);
            }
            else
            {
                return string.Empty;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

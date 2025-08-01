﻿// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Project.Servos;
using Awb.Core.Tools.Validation;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace AwbStudio.ProjectConfiguration.PropertyEditors
{
    public partial class SinglePropertyEditorControl : UserControl, INotifyPropertyChanged
    {
        // define a delegate to return the actual servo position
        public delegate void ActualServoPositionReceivedDelegate(int? servoPositionAbsolute);

        public sealed class GetActualServoPositionEventArgs : EventArgs
        {
            public required ActualServoPositionReceivedDelegate ServoPositionReceivedDelegate { get; set; }

        }

        public EventHandler<GetActualServoPositionEventArgs>? GetActualServoPositionRequested;

        private object? _targetObject;
        private string? _propertyName;
        private PropertyValidator? _propertyValidator;

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
            get
            {
                return _errorMessagesJoined;
            }
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

        public SinglePropertyEditorControl()
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
        public void SetPropertyToEditByName(object targetObject, string propertyName, string? title)
        {
            _targetObject = targetObject;
            _propertyName = propertyName;
            _propertyValidator = new PropertyValidator(_targetObject, propertyName);

            var prop = _targetObject.GetType().GetProperty(propertyName);
            if (prop == null) throw new Exception($"Property '{propertyName}' not found");

            // get the title of the property using the DisplayName annotation
            PropertyTitle = title ?? propertyName;

            // get get description of the property using the Description annotation
            var descriptionAttribute = prop.GetCustomAttribute<DescriptionAttribute>();
            PropertyDescription = descriptionAttribute?.Description ?? string.Empty;

            // activate the correct editor control for this type of property
            // e.g. a checkbox for bool, a textbox for string, a numeric up down for int, etc.
            var value = prop.GetValue(_targetObject); // get the value of the property

            VisualizeValue(prop, value);

            // special handling for servo properties
            var canTakeServoPosition = prop.GetCustomAttribute<SupportsTakeOverTheCurrentServoValueAttribute>();
            ButtonTakeActualServoPosition.Visibility = canTakeServoPosition == null ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;

            ErrorMessagesJoined = _propertyValidator.GetAllErrorMessages(value) ?? string.Empty;
        }

        /// <summary>
        /// Display the current value of the property in the correct editor control
        /// </summary>
        private void VisualizeValue(PropertyInfo prop, object? value)
        {
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
        }

        private void TextBoxPropertyContent_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_propertyValidator == null) throw new NullReferenceException(nameof(_propertyValidator));
            ErrorMessagesJoined = string.Empty;

            // set the value of the property to the value of the textbox,
            // casting to the correct type e.g. int
            if (_targetObject != null && _propertyName != null)
            {
                var newValue = TextPropertyContentTextEditor.Text;

                // check the new value for type compatibility
                var validationAttributeErrors = _propertyValidator.GetErrorMessagesByValidationAttributes(newValue);
                if (validationAttributeErrors != null)
                {
                    ErrorMessagesJoined = validationAttributeErrors;
                    return;
                }

                // try to set the new value and check for errors by the validation attributes
                var typeValidationErrors = _propertyValidator.SetValueAndGetErrorMessagesByType(newValue);
                if (typeValidationErrors != null)
                {
                    ErrorMessagesJoined = typeValidationErrors;
                    return;
                }

                ErrorMessagesJoined = string.Empty;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(_propertyName));
            }
        }

        private void CheckBoxPropertyContentBoolEditor_Unchecked(object sender, System.Windows.RoutedEventArgs e) => SetCheckboxValue(false);
        private void CheckBoxPropertyContentBoolEditor_Checked(object sender, System.Windows.RoutedEventArgs e) => SetCheckboxValue(true);

        private void SetCheckboxValue(bool value)
        {
            ErrorMessagesJoined = string.Empty;

            // set the value of the property to the value of the checkbox
            if (_targetObject != null && _propertyName != null)
            {
                var prop = _targetObject.GetType().GetProperty(_propertyName);
                if (prop == null) throw new Exception($"Property '{_propertyName}' not found");
                prop.SetValue(_targetObject, value);
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(_propertyName));
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ButtonTakeActualServoPosition_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var eventArgs = new GetActualServoPositionEventArgs
            {
                ServoPositionReceivedDelegate = (int? servoPositionAbsolute) =>
                {
                    if (servoPositionAbsolute != null)
                    {
                        // set the value of the property to the value of the textbox,
                        // casting to the correct type e.g. int
                        if (_targetObject != null && _propertyName != null)
                        {
                            var prop = _targetObject.GetType().GetProperty(_propertyName);
                            if (prop == null) throw new Exception($"Property '{_propertyName}' not found");
                            prop.SetValue(_targetObject, servoPositionAbsolute);
                            VisualizeValue(prop, servoPositionAbsolute);
                        }
                    }
                }
            };
            GetActualServoPositionRequested?.Invoke(this, eventArgs);
        }
    }
}

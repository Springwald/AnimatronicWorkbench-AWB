// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Awb.Core.Tools.Validation
{
    public class PropertyValidator
    {
        private readonly object _targetObject;
        private readonly string _propertyName;
        private readonly PropertyInfo _prop;

        public bool IsNullable
        {
            get
            {
                //if (property.PropertyType == typeof(string) &&
                if (_prop.GetMethod?.CustomAttributes.Any(x => x.AttributeType.Name == "NullableContextAttribute") == true)
                    return true;

                // first check if the value is null or empty
                return Nullable.GetUnderlyingType(_prop.PropertyType) != null;
            }
        }

        public PropertyValidator(object targetObject, string propertyName)
        {
            if (targetObject == null) throw new ArgumentNullException(nameof(targetObject));
            if (string.IsNullOrWhiteSpace(propertyName)) throw new ArgumentNullException(nameof(propertyName));
            _targetObject = targetObject;
            _propertyName = propertyName;
            var prop = _targetObject.GetType().GetProperty(_propertyName);
            if (prop == null) throw new Exception($"Property '{_propertyName}' not found");
            _prop = prop;
        }

        /// <summary>
        /// Check a value for this property
        /// </summary>
        public string? GetAllErrorMessages(object? newValue)
        {
            // validate the property value using DataAnnotations (e.g. Range, Required, etc.)
            var attributeErrors = GetErrorMessagesByValidationAttributes(newValue);
            if (!string.IsNullOrWhiteSpace(attributeErrors)) return attributeErrors;

            // validate 
            var typeValueErrors = GetErrorMessagesByType(newValue);
            if (!string.IsNullOrWhiteSpace(typeValueErrors)) return typeValueErrors;

            return null;
        }

        public string? GetErrorMessagesByType(object? newValue) => SetValueAndGetErrorMessagesByType(newValue, setTheValue: false);

        public string? SetValueAndGetErrorMessagesByType(object? newValue) => SetValueAndGetErrorMessagesByType(newValue, setTheValue: true);

        public string? GetErrorMessagesByValidationAttributes(object? value)
        {
            if (_targetObject != null && _propertyName != null)
            {
                var objType = _targetObject.GetType();
                var errors = ValidateProperty(objType, value, _propertyName);
                if (errors.Any())   return string.Join("; ", errors);
            }
            return null;
        }

        private string? SetValueAndGetErrorMessagesByType(object? newValue, bool setTheValue)
        {
            if (_prop == null) throw new ArgumentNullException(nameof(_prop));

            bool isNullable = IsNullable;

            if ((newValue == null) || (newValue is string && string.IsNullOrWhiteSpace(newValue as string)))
            {
                if (isNullable)
                {
                    if (setTheValue)  _prop.SetValue(_targetObject, null);
                    return null;
                }
                else
                    return "Value is empty";
            }

            if (newValue is string newStringValue)
            {
                var propertyType = _prop.PropertyType;

                if (propertyType == typeof(int) || propertyType == typeof(int?))
                {
                    if (int.TryParse(newStringValue, out var intValue))
                    {
                        if (setTheValue) _prop.SetValue(_targetObject, intValue);
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
                        if (setTheValue) _prop.SetValue(_targetObject, uintValue);
                    }
                    else
                    {
                        return "Value is not a valid unsigned integer";
                    }
                }
                else if (propertyType == typeof(string))
                {
                    if (setTheValue) _prop.SetValue(_targetObject, newValue);
                }
                else
                {
                    throw new Exception($"Unsupported property type '{propertyType}'");
                }
            }
            return null;
        }


        // This loop into all DataAnnotations and return all errors strings
        private static IEnumerable<string> ValidateProperty(Type type, object? propertyValue, string propertyName)
        {
            var info = type.GetProperty(propertyName);
            if (info == null) yield break;
            foreach (var va in info.GetCustomAttributes(true).OfType<ValidationAttribute>())
                if (!va.IsValid(propertyValue))
                    yield return va.FormatErrorMessage(string.Empty);
        }

    }
}

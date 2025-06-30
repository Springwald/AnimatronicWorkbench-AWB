// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using System;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;

namespace AwbStudio.WpfTools
{
    /// <summary>
    /// validates a complete class with DataAnnotations
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class GenericValidationRule<T> : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string result = "";
            BindingGroup bindingGroup = (BindingGroup)value;
            foreach (var item in bindingGroup.Items.OfType<T>())
            {
                Type type = typeof(T);
                foreach (var pi in type.GetProperties())
                {
                    foreach (var attrib in pi.GetCustomAttributes(false))
                    {
                        if (attrib is System.ComponentModel.DataAnnotations.ValidationAttribute)
                        {
                            var validationAttribute = attrib as System.ComponentModel.DataAnnotations.ValidationAttribute;
                            if (validationAttribute == null) continue;
                            var val = bindingGroup.GetValue(item, pi.Name);
                            if (!validationAttribute.IsValid(val))
                            {
                                if (result != "")
                                    result += Environment.NewLine;
                                if (string.IsNullOrEmpty(validationAttribute.ErrorMessage))
                                    result += string.Format("Validation on {0} failed!", pi.Name);
                                else
                                    result += validationAttribute.ErrorMessage;
                            }
                        }
                    }
                }
            }
            if (result != "")
                return new ValidationResult(false, result);
            else
                return ValidationResult.ValidResult;
        }
    }
}

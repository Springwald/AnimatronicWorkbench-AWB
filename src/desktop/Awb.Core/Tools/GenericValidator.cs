// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Awb.Core
{
    //internal class ClientValidationRule : GenericValidationRule<Client> { }

    /// <summary>
    /// validates a complete class via DataAnnotations
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GenericValidator<T>
    {
       

        // This loop into all DataAnnotations and return all errors strings
        public IEnumerable<string> ValidateProperty(object? propertyValue, string propertyName)
        {
            Type type = typeof(T);
            var info = type.GetProperty(propertyName);
            if (info == null) yield break;
            foreach (var va in info.GetCustomAttributes(true).OfType<ValidationAttribute>())
                if (!va.IsValid(propertyValue))
                    yield return va.FormatErrorMessage(string.Empty);
        }

        public IEnumerable<string> Validate(object ownerObject, CultureInfo cultureInfo)
        {
            Type type = typeof(T);
            foreach (var propertyInfo in type.GetProperties())
            {
                foreach (var attrib in propertyInfo.GetCustomAttributes(inherit: false))
                {
                    var propertyValue = type.GetProperty(propertyInfo.Name)?.GetValue(ownerObject);
                    var errorInfos = ValidateProperty(propertyValue, propertyInfo.Name);
                    foreach (var errorInfo in errorInfos) yield return errorInfo;
                    /*
                    if (attrib is System.ComponentModel.DataAnnotations.ValidationAttribute)
                    {
                        var validationAttribute = attrib as System.ComponentModel.DataAnnotations.ValidationAttribute;
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
                    }*/
                }
            }
            /*   if (result != "")
                   return new ValidationResult(false, result);
               else
                   return ValidationResult.ValidResult;*/
        }
    }
}
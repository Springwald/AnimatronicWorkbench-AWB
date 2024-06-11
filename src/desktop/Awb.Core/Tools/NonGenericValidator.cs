// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using System.ComponentModel.DataAnnotations;

namespace Awb.Core.Tools
{
    public class NonGenericValidator
    {

        public NonGenericValidator()
        {
                
        }

        // This loop into all DataAnnotations and return all errors strings
        public static IEnumerable<string> ValidateProperty(Type type, object? propertyValue, string propertyName)
        {
            var info = type.GetProperty(propertyName);
            if (info == null) yield break;
            foreach (var va in info.GetCustomAttributes(true).OfType<ValidationAttribute>())
                if (!va.IsValid(propertyValue))
                    yield return va.FormatErrorMessage(string.Empty);
        }
    }
}

// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Project;

namespace Awb.Core.Tools.Validation
{
    public class ObjectValidator
    {
        public static IEnumerable<ProjectProblem> ValidateObjectGetErrors(IProjectObjectListable obj)
        {
            var type = obj.GetType();
            var properties = type.GetProperties();

            foreach (var property in properties)
            {
                var propertyValue = property.GetValue(obj);
                var propertyValidator = new PropertyValidator(obj, property.Name);
                var errorMessagesForProperty = propertyValidator.GetAllErrorMessages(propertyValue);
                if (!string.IsNullOrWhiteSpace(errorMessagesForProperty))
                {
                    yield return new ProjectProblem
                    {
                        ProblemType = ProjectProblem.ProblemTypes.Error,
                        Message = errorMessagesForProperty,
                        Source = $"{obj.TitleShort}: {property.Name}",
                    };
                }
            }
        }
    }
}

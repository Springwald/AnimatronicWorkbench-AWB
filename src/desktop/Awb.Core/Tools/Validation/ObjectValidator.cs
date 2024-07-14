// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Project;
using Awb.Core.Project.Clients;
using Awb.Core.Project.Servos;
using Awb.Core.Project.Various;

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
                //var myValidator = new PropertyValidator(obj, property.Name);
                //var myErrors = myValidator.GetAllErrorMessages(property.GetValue(obj));
                //if (!string.IsNullOrWhiteSpace(myErrors))
                //    yield return myErrors;

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

// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Timelines;
using Awb.Core.Tools.Validation;

namespace Awb.Core.Project
{
    public class CheckProjectIntegrity
    {
        private readonly AwbProject _project;
        private readonly TimelineData[] _timelines;

        public CheckProjectIntegrity(AwbProject awbProject, TimelineData[] timelines)
        {
            _project = awbProject;
            _timelines = timelines;
        }

        public IEnumerable<ProjectProblem> GetProblems()
        {
            //todo: check the project itself

            //if (_project.StsServos != null) foreach (var problem in StsScsServoProblems(_project.StsServos)) yield return problem;
            //if (_project.ScsServos != null) foreach (var problem in StsScsServoProblems(_project.ScsServos)) yield return problem;

            // check the timeline states
            if (_project.TimelinesStates.Any())
            {
                foreach (var state in _project.TimelinesStates)
                    foreach (var problem in state.GetContentProblems(_project)) yield return problem;
            }
            else
            {
                yield return new ProjectProblem { Message = "No timeline states defined", ProblemType = ProjectProblem.ProblemTypes.Error, Source = "Timelines states" };
            }

            var nativeAttributeProblems = _project.GetAllListableObjects().SelectMany(
             x => ObjectValidator.ValidateObjectGetErrors(x));
            foreach (var item in nativeAttributeProblems) yield return item;

            var contentProblems = _project.GetAllListableObjects().SelectMany(x => x.GetContentProblems(_project));
            foreach (var item in contentProblems) yield return item;

            var timelineProblems = _timelines.SelectMany(x => x.GetProblems(_project));
            foreach (var item in timelineProblems) yield return item;

            // check list problems e.g. double IDs
            foreach (var item in GetDoubleIdProblems(_project.Pca9685PwmServos.Select(x => $"Client ID {x.ClientId}, Channel {x.Channel}"), "PCA9685 PWM servos")) yield return item;
            foreach (var item in GetDoubleIdProblems(_project.StsServos.Select(x => $"Client ID {x.ClientId}, Servo ID {x.Channel}"), "STS servos")) yield return item;
            foreach (var item in GetDoubleIdProblems(_project.ScsServos.Select(x => $"Client ID {x.ClientId}, Servo ID {x.Channel}"), "SCS servos")) yield return item;
            foreach (var item in GetDoubleIdProblems(_project.TimelinesStates.Select(x => x.Id.ToString()), "Timeline state IDs")) yield return item;
            foreach (var item in GetDoubleIdProblems(_project.TimelinesStates.Select(x => x.Title), "Timeline state titles")) yield return item;
            foreach (var item in GetDoubleIdProblems(_project.Inputs.Select(x => x.Id.ToString()), "Input IDs")) yield return item;
            foreach (var item in GetDoubleIdProblems(_project.Inputs.Select(x => x.Title), "Input titles")) yield return item;


            //todo: check the pwm servo values
            //todo: check not existing input ids
            //todo: check not existing sound ids
            //todo: check not existing nested timeline ids
        }

        private IEnumerable<ProjectProblem> GetDoubleIdProblems(IEnumerable<string> list, string listName)
        {
            var unique = list.Distinct();
            foreach (var item in unique)
            {
                if (list.Where(x => x == item).Count() > 1)
                {
                    yield return new ProjectProblem()
                    {
                        Message = $"Item '{item}' in list '{listName} ' is not unique!",
                        ProblemType = ProjectProblem.ProblemTypes.Error,
                        Source = listName
                    };
                }
            }
        }



    }
}

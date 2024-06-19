// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

namespace Awb.Core.Project
{
    internal class CheckProjectIntegrity
    {
        private readonly AwbProject _project;

        public IEnumerable<ProjectProblem> GetProblems()
        {
            //todo: check the project itself

            //if (_project.StsServos != null) foreach (var problem in StsScsServoProblems(_project.StsServos)) yield return problem;
            //if (_project.ScsServos != null) foreach (var problem in StsScsServoProblems(_project.ScsServos)) yield return problem;

            // check the timeline states
            if (_project.TimelinesStates != null)
                foreach (var state in _project.TimelinesStates)
                    foreach (var problem in state.GetContentProblems(_project)) yield return problem;

            //todo: check the pwm servo values
            //todo: check double servo ids
            //todo: check not existing input ids
            //todo: check not existing sound ids
            //todo: check not existing nested timeline ids
        }

        public CheckProjectIntegrity(AwbProject awbProject)
        {
            _project = awbProject;
        }

    }
}

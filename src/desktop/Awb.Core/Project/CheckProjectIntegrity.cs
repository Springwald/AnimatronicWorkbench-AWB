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

        public IEnumerable<string> Problems
        {
            get
            {
                if (_project.StsServos != null) foreach (var problem in StsScsServoProblems(_project.StsServos)) yield return problem;
                if (_project.ScsServos != null) foreach (var problem in StsScsServoProblems(_project.ScsServos)) yield return problem;

                // check the timeline states
                if (_project.TimelinesStates != null)
                    foreach (var state in _project.TimelinesStates)
                    {
                        if (state.PositiveInputs != null)
                        {
                            if (state.PositiveInputs.Length > 1)
                                yield return $"TimelineState [{state.Id}] {state.Name} has more than 1 positive input. Actually only 1 is supported.";
                        }
                        if (state.NegativeInputs != null)
                        {
                            if (state.NegativeInputs.Length > 1)
                                yield return $"TimelineState [{state.Id}] {state.Name} has more than 1 negative inputs. Actually only 1 is supported.";
                        }
                    }

                //todo: check the pwm servo values
                //todo: check double servo ids
                //todo: check not existing input ids
                //todo: check not existing sound ids
                //todo: check not existing nested timeline ids
            }
        }

        public CheckProjectIntegrity(AwbProject awbProject)
        {
            _project = awbProject;
        }

        private IEnumerable<string> StsScsServoProblems(StsServoConfig[] stsServoConfigs)
        {
            foreach (var servo in stsServoConfigs)
            {
                if (servo != null)
                {
                    if (servo.MaxValue == servo.MinValue)
                        yield return $"MaxValue and MinValue of servo [{servo.Id}] {servo.Name} are equal!";
                    if (servo.MaxValue < servo.MinValue)
                        yield return $"MaxValue < MinValue at servo [{servo.Id}] {servo.Name}!";
                    if (servo.DefaultValue != null)
                    {
                        if (servo.DefaultValue < servo.MinValue)
                            yield return $"DefaultValue < MinValue at servo [{servo.Id}] {servo.Name}!";
                        if (servo.DefaultValue > servo.MaxValue)
                            yield return $"DefaultValue > MaxValue at servo [{servo.Id}] {servo.Name}!";
                    }
                }
            }
        }

    }
}

﻿// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Configs;

namespace Awb.Core.Project
{
    internal class CheckProjectIntegrity
    {
        private readonly AwbProject _project;

        public IEnumerable<string> Problems
        {
            get
            {
                if (_project.StsServos != null)
                    foreach (var servo in _project.StsServos)
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
                yield break;
            }
        }

        public CheckProjectIntegrity(AwbProject awbProject)
        {
            _project = awbProject;
        }
    }
}

﻿// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

namespace Awb.Core.Project
{
    public class ProjectProblem
    {
        public enum Categories
        {
            Input,
            Servo,
            Sound,
            Timeline,
            TimelineState,
            Various,
        }

        public enum ProblemTypes
        {
            Error,
            UncriticalProblem,
            Hint
        }

        /// <summary>
        /// the problem type - how critical is this problem?
        /// </summary>
        public required ProblemTypes ProblemType { get; init; }

        /// <summary>
        /// thematic category like "Servo" or "Timeline"
        /// </summary>
        public Categories Category { get; init; } = Categories.Various;

        /// <summary>
        /// The name of the object that has the problem
        /// </summary>
        public string? Source { get; init; }

        /// <summary>
        /// a message that describes the problem
        /// </summary>
        public required string Message { get; init; }

        public string PlaintTextDescription => $"{Category} {Source} [{ProblemType.ToString()}]: {Message}";
    }
}

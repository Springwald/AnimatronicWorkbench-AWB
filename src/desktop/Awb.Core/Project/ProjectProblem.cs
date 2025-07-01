// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

namespace Awb.Core.Project
{
    public class ProjectProblem
    {
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
        /// The name of the object that has the problem
        /// </summary>
        public string? Source { get; init; }

        /// <summary>
        /// a message that describes the problem
        /// </summary>
        public required string Message { get; init; }

        public string PlaintTextDescription => $"{Source} [{ProblemType.ToString()}]: {Message}";
    }
}

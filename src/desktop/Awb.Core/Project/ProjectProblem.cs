// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

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

        public required ProblemTypes ProblemType { get; init; }

        public required string ProblemText { get; init; }

    }
}

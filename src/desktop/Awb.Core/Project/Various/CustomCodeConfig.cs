// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License


namespace Awb.Core.Project.Various
{
    internal class CustomCodeConfig : IProjectObjectListable
    {
        public string TitleShort => "Custom code";

        public string TitleDetailed => TitleShort;

        public IEnumerable<ProjectProblem> GetContentProblems(AwbProject project)
        {
            // content specific problems if needed
            yield break;
        }
    }
}

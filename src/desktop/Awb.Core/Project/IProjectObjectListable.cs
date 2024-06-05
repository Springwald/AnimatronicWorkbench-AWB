// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

namespace Awb.Core.Project
{
    public interface IProjectObjectListable
    {
        /// <summary>
        /// a displayable short title e.g to list in the project configuration window
        /// </summary>
        public string TitleShort { get; }

        /// <summary>
        /// a displayable long title e.g to show details or log problems
        /// </summary>
        public string TitleDetailled { get; }
    }
}

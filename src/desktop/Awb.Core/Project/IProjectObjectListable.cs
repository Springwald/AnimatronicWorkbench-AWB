// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Awb.Core.Project
{
    internal interface IProjectObjectListable
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

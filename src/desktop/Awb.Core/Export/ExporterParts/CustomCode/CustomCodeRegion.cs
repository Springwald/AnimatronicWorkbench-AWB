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

namespace Awb.Core.Export.ExporterParts.CustomCode
{
    internal class CustomCodeRegion
    {
        public string Filename { get; init; }
        public string RegionName { get; init; }
        public string Content { get; init; }
    }
}

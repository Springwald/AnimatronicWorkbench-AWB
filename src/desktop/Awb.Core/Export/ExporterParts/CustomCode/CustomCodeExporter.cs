﻿// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using static Awb.Core.Export.IExporter;

namespace Awb.Core.Export.ExporterParts.CustomCode
{
    internal class CustomCodeExporter : ExporterPartAbstract
    {
        public override async Task<ExportResult> ExportAsync(string targetSrcFolder)
        {
            return ExportResult.SuccessResult;
        }
    }
}
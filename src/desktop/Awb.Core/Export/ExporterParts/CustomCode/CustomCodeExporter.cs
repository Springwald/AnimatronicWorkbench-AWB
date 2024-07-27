// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using static Awb.Core.Export.IExporter;

namespace Awb.Core.Export.ExporterParts.CustomCode
{
    internal class CustomCodeExporter : ExporterPartAbstract
    {
        public override async Task<ExportResult> ExportAsync(string targetSrcFolder)
        {
            // backup existing custom code files if existing

            // read the regions from existing custom code files

            // read the template file for custom code

            // write the template file to the target folder stamping the read regions into the template

            return ExportResult.SuccessResult;
        }
    }
}

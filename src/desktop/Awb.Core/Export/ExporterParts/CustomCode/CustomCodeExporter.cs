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
        private readonly CustomCodeRegionContent _customCodeRegionContent;
        private readonly string _targetFolder;

        public CustomCodeExporter(CustomCodeRegionContent customCodeRegionContent, string targetFolder)
        {
            _customCodeRegionContent = customCodeRegionContent;
            _targetFolder = targetFolder;
        }

        public override async Task<ExportResult> ExportAsync()
        {
            // check the target folder
            if (!Directory.Exists(_targetFolder)) return new IExporter.ExportResult { ErrorMessage = $"Target folder '{_targetFolder}' not found" };

            var files = _customCodeRegionContent.Regions.GroupBy(r => r.Filename);

            // read the template file for custom code


            // write the template file to the target folder stamping the read regions into the template

            return ExportResult.SuccessResult;
        }
    }
}

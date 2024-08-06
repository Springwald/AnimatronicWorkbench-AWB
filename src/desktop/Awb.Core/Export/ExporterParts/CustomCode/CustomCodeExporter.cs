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
            var customCoderReaderWriter = new CustomCodeReaderWriter();
            foreach (var file in files)
            {
                var templateFile = Path.Combine(_targetFolder, file.Key);
                if (!File.Exists(templateFile)) return new IExporter.ExportResult { ErrorMessage = $"Custom code template file '{templateFile}' not found" };

                // read the template file
                var templateContent = await File.ReadAllTextAsync(templateFile);

                // replace the custom code regions in the template file
                var writerResult = customCoderReaderWriter.WriteRegions(templateContent: templateContent, regionContent: _customCodeRegionContent);
                if (writerResult.Success == false) return new ExportResult { ErrorMessage = writerResult.ErrorMsg };

                // write the custom code back into the file
                await File.WriteAllTextAsync(templateFile, writerResult.Content);
            }

            return ExportResult.SuccessResult;
        }
    }
}

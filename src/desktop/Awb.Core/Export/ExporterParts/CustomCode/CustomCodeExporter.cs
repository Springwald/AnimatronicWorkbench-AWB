// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

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

            InvokeProcessing(new ExporterProcessStateEventArgs { State = ExporterProcessStateEventArgs.ProcessStates.OnlyLog, Message = $"\r\n-------------------------------------" });
            InvokeProcessing(new ExporterProcessStateEventArgs { State = ExporterProcessStateEventArgs.ProcessStates.OnlyLog, Message = $"## Export custom code to folder '{_targetFolder}'" });

            var filesReadBefore = _customCodeRegionContent.Regions.Select(r => r.Filename).Distinct();
            InvokeProcessing(new ExporterProcessStateEventArgs { State = ExporterProcessStateEventArgs.ProcessStates.OnlyLog, Message = $"## filesReadBefore: {string.Join(", ", filesReadBefore.Select(f => $"'{f}'"))}" });

            // read all files in the target folder that are .h or .cpp files
            var filesInTargetDir = Directory.GetFiles(_targetFolder, "*.h").Concat(Directory.GetFiles(_targetFolder, "*.cpp")).ToList();
            InvokeProcessing(new ExporterProcessStateEventArgs { State = ExporterProcessStateEventArgs.ProcessStates.OnlyLog, Message = $"## filesInTargetDir: {string.Join(", ", filesInTargetDir.Select(f => $"'{f}'"))}" });

            // read the template file for custom code
            var customCoderReaderWriter = new CustomCodeReaderWriter();
            customCoderReaderWriter.Processing += CustomCoderReaderWriter_Processing;
            foreach (var file in filesInTargetDir)
            {
                var fileInfo = new FileInfo(file);

                // read the template file
                var templateFilename = Path.Combine(_targetFolder, fileInfo.Name);
                var templateContent = await File.ReadAllTextAsync(templateFilename);
                InvokeProcessing(new ExporterProcessStateEventArgs { State = ExporterProcessStateEventArgs.ProcessStates.OnlyLog, Message = $"\r\n-------------------------------------" });
                InvokeProcessing(new ExporterProcessStateEventArgs { State = ExporterProcessStateEventArgs.ProcessStates.OnlyLog, Message = $"## Read template file '{templateFilename}'" });

                // remove the file from the list of files read before
                filesReadBefore = filesReadBefore.Where(f => f != fileInfo.Name);

                // replace the custom code regions in the template file
                var writerResult = customCoderReaderWriter.WriteRegions(filename: fileInfo.Name, templateContent: templateContent, regionContent: _customCodeRegionContent);
                if (writerResult.Success == false) return new ExportResult { ErrorMessage = writerResult.ErrorMsg };

                // write the custom code back into the file
                await File.WriteAllTextAsync(templateFilename, writerResult.Content);
            }

            InvokeProcessing(new ExporterProcessStateEventArgs { State = ExporterProcessStateEventArgs.ProcessStates.OnlyLog, Message = $"## filesReadBefore left: {string.Join(", ", filesReadBefore.Select(f => $"'{f}'"))}" });
            if (filesReadBefore.Any())
                return new IExporter.ExportResult { ErrorMessage = $"Custom code template files {string.Join(", ", filesReadBefore.Select(f => $"'{f}'"))}" };


            return ExportResult.SuccessResult;
        }

        private void CustomCoderReaderWriter_Processing(object? sender, ExporterProcessStateEventArgs e) => InvokeProcessing(e);
    }
}

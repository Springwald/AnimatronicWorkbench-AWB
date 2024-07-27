// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License


using Awb.Core.Export.ExporterParts;
using Awb.Core.Export.ExporterParts.CustomCode;

namespace Awb.Core.Export
{
    public class Esp32ClientExporter : IExporter
    {
        private readonly string _esp32ClientsSourceFolder;
        private readonly WifiConfigExportData _wifiConfigData;
        private readonly ProjectExportData _projectExportData;

        public string Title { get; } = "ESP32 Client";

        public event EventHandler<ExporterProcessStateEventArgs>? Processing;

        public Esp32ClientExporter(string esp32ClientsSourceFolder, WifiConfigExportData wifiConfigData, ProjectExportData projectExportData)
        {
            _esp32ClientsSourceFolder = esp32ClientsSourceFolder;
            _wifiConfigData = wifiConfigData;
            _projectExportData = projectExportData;
        }

        public async Task<IExporter.ExportResult> ExportAsync(string targetPath, string projectFolder)
        {
            var remoteSrcFolder = Path.Combine(_esp32ClientsSourceFolder, "awb_esp32_client");
            var customCodeTargetFolder = Path.Combine(targetPath, @"src\AwbDataImport\CustomCode");
;
            if (!Directory.Exists(remoteSrcFolder))
            {
                return new IExporter.ExportResult { ErrorMessage = $"Source folder '{remoteSrcFolder}' not found" };
            }

            CustomCodeRegionContent? customCodeRegionContent = null;
            var customCodeBackupRootBolderFolder = Path.Combine(projectFolder, "custom_code_backup");
            var customCodeBackup = new CustomCodeBackup(customCodeTargetFolder: customCodeTargetFolder, customCodeBackupRootFolder: customCodeBackupRootBolderFolder);
            if (customCodeBackup.CustomCodeExists)
            {
                var customCodeBackupResult = customCodeBackup.Backup();
                if (!customCodeBackupResult.Success)
                {
                    Processing?.Invoke(this, new ExporterProcessStateEventArgs { ErrorMessage = customCodeBackupResult.ErrorMsg });
                    return new IExporter.ExportResult { ErrorMessage = customCodeBackupResult.ErrorMsg };
                }
                // take the existing custom code regions
                customCodeRegionContent = customCodeBackupResult.CustomCodeRegionContent;
            } else
            {
                // seems as if there is no custom code yet, so we create an empty custom code region content
                customCodeRegionContent = new CustomCodeRegionContent();
            }

            // copy the template source folder to the target folder
            var cloneResult = await new SrcFolderCloner(remoteSrcFolder, targetPath, removeExtraFilesInTarget: false).Clone();
            if (!cloneResult.Success)
            {
                Processing?.Invoke(this, new ExporterProcessStateEventArgs { ErrorMessage = cloneResult.ErrorMessage });
                return new IExporter.ExportResult { ErrorMessage = cloneResult.ErrorMessage };
            }

            // write the exported data to the target folder and overwrite the template files with the exported data
            var dataExporter = new ExporterPartAbstract[]
            {
                // announce the different exporters
                new WifiConfigExporter(_wifiConfigData),
                new ProjectDataExporter(_projectExportData),
                new CustomCodeExporter()
            };

            // export the data using the exporters
            foreach (var exporter in dataExporter)
            {
                var result = await exporter.ExportAsync(targetPath);
                if (!result.Success)
                {
                    Processing?.Invoke(this, new ExporterProcessStateEventArgs { ErrorMessage =  result.ErrorMessage });
                    return result;
                }
            }

            return IExporter.ExportResult.SuccessResult;
        }
    }
}

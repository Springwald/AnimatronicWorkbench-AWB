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
    public abstract class MainExporterAbstract : IExporter
    {
        protected readonly string _projectFolder;
        protected readonly string _esp32ClientsTemplateSourceFolder;
        protected const string _customCodeFolderName = "CustomCode";

        protected string? CustomCodeTargetFolder { get; private set; }

        protected CustomCodeRegionContent? CustomCodeRegionContent { get; set; } = null;

        public abstract string TemplateSourceFolderRelative { get; }

        protected string Esp32TemplateSourceFolderAbsolute => Path.Combine(_esp32ClientsTemplateSourceFolder, TemplateSourceFolderRelative);

        public event EventHandler<ExporterProcessStateEventArgs>? Processing;

        public abstract string Title { get; }

        protected MainExporterAbstract(string projectFolder, string esp32ClientsTemplateSourceFolder)
        {
            if (string.IsNullOrWhiteSpace(projectFolder)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(projectFolder));
            if (string.IsNullOrWhiteSpace(esp32ClientsTemplateSourceFolder)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(esp32ClientsTemplateSourceFolder));

            _projectFolder = projectFolder;
            _esp32ClientsTemplateSourceFolder = esp32ClientsTemplateSourceFolder;
        }

        protected abstract ExporterPartAbstract[] GetExporterParts(string targetPath);

        protected void InvokeProcessing(ExporterProcessStateEventArgs eventArgs) => Processing?.Invoke(this, eventArgs);

        public async Task<IExporter.ExportResult> ExportAsync(string targetPath)
        {
            if (string.IsNullOrWhiteSpace(targetPath)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(targetPath));

            // Read the existing custom code in the target folder and backup it
            CustomCodeTargetFolder = Path.Combine(targetPath, @"src\AwbDataImport", _customCodeFolderName);
            var readAndBackupCustomCodeResult = await ReadAndBackupCustomCodeAsync();
            if (readAndBackupCustomCodeResult.Success == false) return readAndBackupCustomCodeResult;

            // check if the the template source folder exists
            if (!Directory.Exists(Esp32TemplateSourceFolderAbsolute))
                return new IExporter.ExportResult { ErrorMessage = $"Template source folder '{Esp32TemplateSourceFolderAbsolute}' not found" };

            // copy the template source folder to the target folder
             var cloner = new SrcFolderCloner(
                sourceFolder: Esp32TemplateSourceFolderAbsolute,
                targetFolder: targetPath,
                removeExtraFilesInTarget: true,
                removeFilesBlockerDirectoryNames: new string[] { _customCodeFolderName , ".pio"}
                );
            cloner.Processing += (sender, e) => Processing?.Invoke(this, e);
            var cloneResult = await cloner.Clone();
            cloner.Processing -= (sender, e) => Processing?.Invoke(this, e);
            if (!cloneResult.Success)
            {
                InvokeProcessing(new ExporterProcessStateEventArgs { ErrorMessage = cloneResult.ErrorMessage });
                return new IExporter.ExportResult { ErrorMessage = cloneResult.ErrorMessage };
            }

            // export the data using the exporters
            foreach (var exporter in GetExporterParts(targetPath))
            {
                var result = await exporter.ExportAsync();
                if (!result.Success)
                {
                    InvokeProcessing(new ExporterProcessStateEventArgs { ErrorMessage = result.ErrorMessage });
                    return result;
                }
            }

            return IExporter.ExportResult.SuccessResult;
        }


        /// <summary>
        /// Read existing custom code in the target folder and backup it
        /// </summary>
        protected async Task<IExporter.ExportResult> ReadAndBackupCustomCodeAsync()
        {
            var customCodeBackupRootBolderFolder = Path.Combine(_projectFolder, "custom_code_backup", TemplateSourceFolderRelative);

            // Create the custom code backup folder if it does not exist
            if (!Directory.Exists(customCodeBackupRootBolderFolder))
            {
                if (!Directory.Exists(_projectFolder))
                {
                    Processing?.Invoke(this, new ExporterProcessStateEventArgs { ErrorMessage = "Project folder not found: " + _projectFolder });
                    return new IExporter.ExportResult { ErrorMessage = "Project folder not found: " + _projectFolder };
                }

                try
                {
                    Directory.CreateDirectory(customCodeBackupRootBolderFolder);
                }
                catch (Exception ex)
                {
                    Processing?.Invoke(this, new ExporterProcessStateEventArgs { ErrorMessage = "Error creating custom code backup folder: " + ex.Message });
                    return new IExporter.ExportResult { ErrorMessage = "Error creating custom code backup folder: " + ex.Message };
                }
            }

            // Check if custom code exists and backup it and read the existing custom code regions into CustomCodeRegionContent
            var customCodeBackup = new CustomCodeBackup(customCodeTargetFolder: CustomCodeTargetFolder, customCodeBackupRootFolder: customCodeBackupRootBolderFolder);
            if (customCodeBackup.CustomCodeExists)
            {
                var customCodeBackupResult = customCodeBackup.Backup();
                if (!customCodeBackupResult.Success || customCodeBackupResult.CustomCodeRegionContent == null)
                {
                    Processing?.Invoke(this, new ExporterProcessStateEventArgs { ErrorMessage = customCodeBackupResult.ErrorMsg });
                    return new IExporter.ExportResult { ErrorMessage = customCodeBackupResult.ErrorMsg };
                }
                // take the existing custom code regions
                CustomCodeRegionContent = customCodeBackupResult.CustomCodeRegionContent;
            }
            else
            {
                // seems as if there is no custom code yet, so we create an empty custom code region content
                CustomCodeRegionContent = new CustomCodeRegionContent();
            }

            await Task.CompletedTask;
            return IExporter.ExportResult.SuccessResult;
        }
    }
}

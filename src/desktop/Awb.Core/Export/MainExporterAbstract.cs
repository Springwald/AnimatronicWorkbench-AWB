// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License


using Awb.Core.Export.ExporterParts;
using Awb.Core.Export.ExporterParts.CustomCode;
using System.Text;

namespace Awb.Core.Export
{
    public abstract class MainExporterAbstract : IExporter
    {

        private StringBuilder _logContent = new StringBuilder();

        protected readonly string _projectFolder;
        protected readonly string _esp32ClientsTemplateSourceFolder;
        protected const string _customCodeFolderName = "CustomCode";

        protected string? CustomCodeTargetFolder { get; private set; }

        protected CustomCodeRegionContent? CustomCodeRegionContent { get; set; } = null;

        public abstract string TemplateSourceFolderRelative { get; }

        protected string Esp32TemplateSourceFolderAbsolute => Path.Combine(_esp32ClientsTemplateSourceFolder, TemplateSourceFolderRelative);

        public event EventHandler<ExporterProcessStateEventArgs>? ProcessingState;

        public void InvokeProcessing(ExporterProcessStateEventArgs eventArgs)
        {
            ProcessingState?.Invoke(this, eventArgs);
            switch (eventArgs.State)
            {
                case ExporterProcessStateEventArgs.ProcessStates.Error:
                    _logContent.AppendLine($"Error: {eventArgs.Message}");
                    break;
                case ExporterProcessStateEventArgs.ProcessStates.OnlyLog:
                    _logContent.AppendLine(eventArgs.Message);
                    break;
                case ExporterProcessStateEventArgs.ProcessStates.Message:
                    _logContent.AppendLine(eventArgs.Message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(Title, "Unknown ExporterProcessStateEventArgs.ProcessStates: " + eventArgs.State.ToString());
            }
        }


        public abstract string Title { get; }

        protected MainExporterAbstract(string projectFolder, string esp32ClientsTemplateSourceFolder)
        {
            if (string.IsNullOrWhiteSpace(projectFolder)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(projectFolder));
            if (string.IsNullOrWhiteSpace(esp32ClientsTemplateSourceFolder)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(esp32ClientsTemplateSourceFolder));

            _projectFolder = projectFolder;
            _esp32ClientsTemplateSourceFolder = esp32ClientsTemplateSourceFolder;
        }

        protected abstract ExporterPartAbstract[] GetExporterParts(string targetPath);

        public async Task<IExporter.ExportResult> ExportAsync(string targetPath)
        {
            InvokeProcessing(new ExporterProcessStateEventArgs { Message = $"Exporting '{Title}' to '{targetPath}'", State = ExporterProcessStateEventArgs.ProcessStates.OnlyLog });

            if (string.IsNullOrWhiteSpace(targetPath)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(targetPath));

            // Read the existing custom code in the target folder and backup it
            CustomCodeTargetFolder = Path.Combine(targetPath, @"src", _customCodeFolderName);
            var readAndBackupCustomCodeResult = await ReadAndBackupCustomCodeAsync();
            if (readAndBackupCustomCodeResult.Success == false)
            {
                await WriteLog(targetPath);
                return readAndBackupCustomCodeResult;
            }

            // check if the the template source folder exists
            if (!Directory.Exists(Esp32TemplateSourceFolderAbsolute))
            {
                await WriteLog(targetPath);
                return new IExporter.ExportResult { ErrorMessage = $"Template source folder '{Esp32TemplateSourceFolderAbsolute}' not found" };
            }

            // copy the template source folder to the target folder
            var cloner = new SrcFolderCloner(
               sourceFolder: Esp32TemplateSourceFolderAbsolute,
               targetFolder: targetPath,
               removeExtraFilesInTarget: true,
               removeFilesBlockerDirectoryNames: new string[] { _customCodeFolderName, ".pio", ".vscode" },
               copyFilesBlockerDirectoryNames: new string[] { ".pio", ".vscode" }
               );
            cloner.Processing += (sender, e) => InvokeProcessing(e);
            var cloneResult = await cloner.Clone();
            cloner.Processing -= (sender, e) => InvokeProcessing(e);
            if (!cloneResult.Success)
            {
                InvokeProcessing(new ExporterProcessStateEventArgs { Message = cloneResult.ErrorMessage, State = ExporterProcessStateEventArgs.ProcessStates.Error });
                await WriteLog(targetPath);
                return new IExporter.ExportResult { ErrorMessage = cloneResult.ErrorMessage };
            }

            // export the data using the exporters
            foreach (var exporter in GetExporterParts(targetPath))
            {
                exporter.Processing += (sender, e) => InvokeProcessing(e);
                var result = await exporter.ExportAsync();
                exporter.Processing -= (sender, e) => InvokeProcessing(e);
                if (!result.Success)
                {
                    InvokeProcessing(new ExporterProcessStateEventArgs { Message = result.ErrorMessage!, State = ExporterProcessStateEventArgs.ProcessStates.Error });
                    await WriteLog(targetPath);
                    return result;
                }
            }

            // Write exporter log
            InvokeProcessing(new ExporterProcessStateEventArgs { Message = "Export done.", State = ExporterProcessStateEventArgs.ProcessStates.OnlyLog });
            await WriteLog(targetPath);

            return IExporter.ExportResult.SuccessResult;
        }

        private async Task WriteLog(string targetPath)
        {
            var logFilename = Path.Combine(targetPath, "export.log");
            try
            {
                await File.WriteAllTextAsync(logFilename, _logContent.ToString());
            }
            catch (Exception)
            {
            }
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
                    InvokeProcessing(new ExporterProcessStateEventArgs { Message = "Project folder not found: " + _projectFolder, State = ExporterProcessStateEventArgs.ProcessStates.Error });
                    return new IExporter.ExportResult { ErrorMessage = "Project folder not found: " + _projectFolder };
                }

                try
                {
                    Directory.CreateDirectory(customCodeBackupRootBolderFolder);
                    InvokeProcessing(new ExporterProcessStateEventArgs { Message = "Project folder created: " + _projectFolder, State = ExporterProcessStateEventArgs.ProcessStates.OnlyLog });
                }
                catch (Exception ex)
                {
                    InvokeProcessing(new ExporterProcessStateEventArgs { Message = "Error creating custom code backup folder: " + ex.Message, State = ExporterProcessStateEventArgs.ProcessStates.Error });
                    return new IExporter.ExportResult { ErrorMessage = "Error creating custom code backup folder: " + ex.Message };
                }
            }

            // Check if custom code exists and backup it and read the existing custom code regions into CustomCodeRegionContent
            var customCodeBackup = new CustomCodeBackup(customCodeTargetFolder: CustomCodeTargetFolder, customCodeBackupRootFolder: customCodeBackupRootBolderFolder);
            customCodeBackup.Processing += CustomCodeBackup_Processing;
            if (customCodeBackup.CustomCodeExists)
            {
                InvokeProcessing(new ExporterProcessStateEventArgs { Message = "Custom code already exists.", State = ExporterProcessStateEventArgs.ProcessStates.OnlyLog });
                var customCodeBackupResult = customCodeBackup.Backup();
                if (!customCodeBackupResult.Success || customCodeBackupResult.CustomCodeRegionContent == null)
                {
                    InvokeProcessing(new ExporterProcessStateEventArgs { Message = customCodeBackupResult.ErrorMsg, State = ExporterProcessStateEventArgs.ProcessStates.Error });
                    return new IExporter.ExportResult { ErrorMessage = customCodeBackupResult.ErrorMsg };
                }
                // take the existing custom code regions
                CustomCodeRegionContent = customCodeBackupResult.CustomCodeRegionContent;
            }
            else
            {
                // seems as if there is no custom code yet, so we create an empty custom code region content
                InvokeProcessing(new ExporterProcessStateEventArgs { Message = "no custom code yet", State = ExporterProcessStateEventArgs.ProcessStates.OnlyLog });
                CustomCodeRegionContent = new CustomCodeRegionContent();
            }

            await Task.CompletedTask;
            return IExporter.ExportResult.SuccessResult;
        }

        private void CustomCodeBackup_Processing(object? sender, ExporterProcessStateEventArgs e) => InvokeProcessing(e);
    }
}

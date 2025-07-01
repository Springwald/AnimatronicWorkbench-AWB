// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

namespace Awb.Core.Export.ExporterParts.CustomCode
{
    public class CustomCodeBackup
    {
        private readonly string _customCodeFolder;
        private readonly string _customCodeBackupRootFolder;

        public event EventHandler<ExporterProcessStateEventArgs>? Processing;

        public CustomCodeBackup(string customCodeTargetFolder, string customCodeBackupRootFolder)
        {
            if (string.IsNullOrWhiteSpace(customCodeTargetFolder)) throw new ArgumentException("Value cannot be empty.", nameof(customCodeTargetFolder));
            if (string.IsNullOrWhiteSpace(customCodeBackupRootFolder)) throw new ArgumentException("Value cannot be empty.", nameof(customCodeBackupRootFolder));

            if (!Directory.Exists(customCodeBackupRootFolder)) throw new ArgumentException("Folder '" + customCodeBackupRootFolder + "' does not exist", nameof(customCodeBackupRootFolder));

            _customCodeFolder = customCodeTargetFolder;
            _customCodeBackupRootFolder = customCodeBackupRootFolder;
        }

        public bool CustomCodeExists => Directory.Exists(_customCodeFolder);

        public CustomCodeBackupResult Backup()
        {
            if (!CustomCodeExists) return new CustomCodeBackupResult { Success = false, ErrorMsg = "No custom code found" };
            var customCodeRegionContent = new CustomCodeRegionContent();

            var backupFolder = Path.Combine(_customCodeBackupRootFolder, "Backup_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            Directory.CreateDirectory(backupFolder);

            // find all files in the custom code folder
            var customCodeReaderWriter = new CustomCodeReaderWriter();
            customCodeReaderWriter.Processing += CustomCodeReaderWriter_Processing;
            var files = Directory.GetFiles(_customCodeFolder, "*.*", SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                switch (fileInfo.Extension.ToLower())
                {
                    case ".cpp":
                    case ".h":
                        break;
                    default:
                        return new CustomCodeBackupResult { Success = false, ErrorMsg = "Unsupported custom code file extension: " + fileInfo.Extension };
                }

                // read the regions from the file
                var fileContent = File.ReadAllText(file);
                Processing?.Invoke(this, new ExporterProcessStateEventArgs { State = ExporterProcessStateEventArgs.ProcessStates.OnlyLog, Message = $"\r\n----------------------------------------------------" });
                Processing?.Invoke(this, new ExporterProcessStateEventArgs { State = ExporterProcessStateEventArgs.ProcessStates.OnlyLog, Message = $"## Opening file '{file}'" });
                var regionsReadResult = customCodeReaderWriter.ReadRegions(filename: fileInfo.Name, content: fileContent);
                if (regionsReadResult.ErrorMsg != null) return new CustomCodeBackupResult { Success = false, ErrorMsg = regionsReadResult.ErrorMsg };

                // save the regions in the custom code region content result
                foreach (var region in regionsReadResult.Regions)
                    customCodeRegionContent.AddRegion(filename: fileInfo.Name, key: region.Key, content: region.Content);

                // write the file to the backup folder
                var backupFilename = Path.Combine(backupFolder, fileInfo.Name);
                File.WriteAllText(backupFilename, fileContent);
            }

            return new CustomCodeBackupResult { Success = true, CustomCodeRegionContent = customCodeRegionContent };
        }

        private void CustomCodeReaderWriter_Processing(object? sender, ExporterProcessStateEventArgs e) => Processing?.Invoke(this, e);
    }
}

// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

namespace Awb.Core.Export.ExporterParts.CustomCode
{
    public class CustomCodeBackup
    {
        private readonly string _customCodeFolder;
        private readonly string _customCodeBackupRootFolder;

        public class BackupResult
        {
            public required bool Success { get; init; }
            public CustomCodeRegionContent? CustomCodeRegionContent{ get; init; }
            public string? ErrorMsg { get; init; }
        }

        public CustomCodeBackup(string customCodeTargetFolder, string customCodeBackupRootFolder)
        {
            if (string.IsNullOrWhiteSpace(customCodeTargetFolder))              throw new ArgumentException("Value cannot be empty.", nameof(customCodeTargetFolder));
            if (string.IsNullOrWhiteSpace(customCodeBackupRootFolder))        throw new ArgumentException("Value cannot be empty.", nameof(customCodeBackupRootFolder));

            if (!Directory.Exists(customCodeBackupRootFolder)) throw new ArgumentException("Folder '" + customCodeBackupRootFolder + "' does not exist", nameof(customCodeBackupRootFolder));

            _customCodeFolder = customCodeTargetFolder;
            _customCodeBackupRootFolder = customCodeBackupRootFolder;
        }

        public bool CustomCodeExists => Directory.Exists(_customCodeFolder);

        public BackupResult Backup()
        {
            if (!CustomCodeExists) return new BackupResult { Success = false, ErrorMsg = "No custom code found" };
            var customCodeRegionContent = new CustomCodeRegionContent();

            var backupFolder = Path.Combine(_customCodeBackupRootFolder, "Backup_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            Directory.CreateDirectory(backupFolder);

            // find all files in the custom code folder
            var customCodeReaderWriter = new CustomCodeReaderWriter();
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
                        return new BackupResult { Success = false, ErrorMsg = "Unsupported custom code file extension: " + fileInfo.Extension };
                }

                // read the regions from the file
                var fileContent = File.ReadAllText(file);
                var regionsReadResult = customCodeReaderWriter.ReadRegions(filename: fileInfo.Name, content: fileContent);
                if (regionsReadResult.ErrorMsg != null) return new BackupResult { Success = false, ErrorMsg = regionsReadResult.ErrorMsg };

                // save the regions in the custom code region content result
                foreach (var region in regionsReadResult.Regions)
                    customCodeRegionContent.AddRegion(filename: fileInfo.Name, key: region.Key, content: region.Content);

                // write the file to the backup folder
                var backupFilename = Path.Combine(backupFolder, fileInfo.Name); 
                File.WriteAllText(backupFilename, fileContent);
            }

            return new BackupResult { Success = true, CustomCodeRegionContent = customCodeRegionContent };
        }
    }
}

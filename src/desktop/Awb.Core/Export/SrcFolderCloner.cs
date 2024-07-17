// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using System.Diagnostics;

namespace Awb.Core.Export
{
    internal class SrcFolderCloner
    {
        public class CloneResult
        {
            public bool Success => string.IsNullOrEmpty(ErrorMessage);
            public string? ErrorMessage { get; set; }
        }

        private readonly string _sourceFolder;
        private readonly string _targetFolder;

        public SrcFolderCloner(string sourceFolder, string targetFolder, bool removeExtraFilesInTarget)
        {
            if (removeExtraFilesInTarget) throw new System.NotImplementedException("removeExtraFilesInTarget not supported yet!");

            _sourceFolder = sourceFolder;
            _targetFolder = targetFolder;
        }

        public async Task<CloneResult> Clone()
        {
            if (!Directory.Exists(_sourceFolder)) return new CloneResult { ErrorMessage = $"Source folder '{_sourceFolder}' does not exist" };
            if (!Directory.Exists(_targetFolder)) return new CloneResult { ErrorMessage = $"Target folder '{_targetFolder}' does not exist" };


            // Delete files not existing in source folder
            foreach (var targetFile in Directory.GetFiles(_targetFolder, "*", SearchOption.AllDirectories))
            {
                var relativePath = targetFile.Substring(_targetFolder.Length + 1);
                var sourceFile = Path.Combine(_sourceFolder, relativePath);

                if (!File.Exists(sourceFile))
                    File.Delete(targetFile);
            }

            // clone the source folder to the target folder stepping through all files and subfolders
            foreach (var sourceFile in Directory.GetFiles(_sourceFolder, "*", SearchOption.AllDirectories))
            {
                var relativePath = sourceFile.Substring(_sourceFolder.Length + 1);
                var targetFile = Path.Combine(_targetFolder, relativePath);

                var targetDir = Path.GetDirectoryName(targetFile);
                if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);

                File.Copy(sourceFile, targetFile, true);
            }
          
            
            return new CloneResult();
        }
    }
}

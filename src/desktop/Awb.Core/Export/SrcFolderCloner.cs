// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

namespace Awb.Core.Export
{
    internal class SrcFolderCloner
    {
        private readonly string _sourceFolder;
        private readonly string _targetFolder;
        private readonly bool _removeExtraFilesInTarget;
        private readonly string[] _removeFilesBlockerDirectoryNames;
        private readonly string[] _copyFilesBlockerDirectoryNames;

        private readonly bool _deepLog = false;

        public class CloneResult
        {
            public bool Success => string.IsNullOrEmpty(ErrorMessage);
            public string? ErrorMessage { get; set; }
        }

        public event EventHandler<ExporterProcessStateEventArgs>? Processing;

        public SrcFolderCloner(string sourceFolder, string targetFolder, bool removeExtraFilesInTarget, string[] removeFilesBlockerDirectoryNames, string[] copyFilesBlockerDirectoryNames)
        {
            _sourceFolder = sourceFolder;
            _targetFolder = targetFolder;
            _removeExtraFilesInTarget = removeExtraFilesInTarget;
            _removeFilesBlockerDirectoryNames = removeFilesBlockerDirectoryNames;
            _copyFilesBlockerDirectoryNames = copyFilesBlockerDirectoryNames;
        }

        public async Task<CloneResult> Clone()
        {
            if (!Directory.Exists(_sourceFolder)) return new CloneResult { ErrorMessage = $"Source folder '{_sourceFolder}' does not exist" };
            if (!Directory.Exists(_targetFolder)) return new CloneResult { ErrorMessage = $"Target folder '{_targetFolder}' does not exist" };

            // remove files in target folder that are not in source folder
            if (_removeExtraFilesInTarget)
            {
                foreach (var targetFile in Directory.GetFiles(_targetFolder, "*", SearchOption.AllDirectories))
                {
                    var relativePath = targetFile.Substring(_targetFolder.Length + 1);
                    var sourceFile = Path.Combine(_sourceFolder, relativePath);

                    if (!File.Exists(sourceFile))
                    {
                        var delete = true;
                        foreach (var blockerDir in _removeFilesBlockerDirectoryNames)
                        {
                            if (sourceFile.Contains($@"\{blockerDir}\"))
                            {
                                if (_deepLog) Processing?.Invoke(this, new ExporterProcessStateEventArgs { Message = $"Skipping deleting file '{targetFile}' because it is located in '{blockerDir}'", State = ExporterProcessStateEventArgs.ProcessStates.OnlyLog });
                                delete = false;
                                break;
                            }
                        }
                        if (delete)
                        {
                            File.Delete(targetFile);
                            Processing?.Invoke(this, new ExporterProcessStateEventArgs { Message = $"Deleted file '{targetFile}' because it is not in source folder.", State = ExporterProcessStateEventArgs.ProcessStates.OnlyLog });
                        }
                    }
                }
            }

            // clone the source folder to the target folder stepping through all files and subfolders
            foreach (var sourceFile in Directory.GetFiles(_sourceFolder, "*", SearchOption.AllDirectories))
            {
                foreach (var blockerDir in _copyFilesBlockerDirectoryNames)
                {
                    if (sourceFile.Contains($@"\{blockerDir}\"))
                    {
                        if (_deepLog) Processing?.Invoke(this, new ExporterProcessStateEventArgs { Message = $"Skipping copying file '{sourceFile}' because it is located in '{blockerDir}'", State = ExporterProcessStateEventArgs.ProcessStates.OnlyLog });
                        continue;
                    }
                }

                var relativePath = sourceFile.Substring(_sourceFolder.Length + 1);
                var targetFile = Path.Combine(_targetFolder, relativePath);

                var targetDir = Path.GetDirectoryName(targetFile);
                if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);


                // copy only if the content of sourceFile and targetFile are different
                var copy = false;

                // check last file writetime
                if (File.Exists(targetFile))
                {
                    var sourceFileTime = File.GetLastWriteTimeUtc(sourceFile);
                    var targetFileTime = File.GetLastWriteTimeUtc(targetFile);
                    if (sourceFileTime != targetFileTime)
                    {
                        copy = true;
                    }
                    else
                    {
                        var sourceFileSize = new FileInfo(sourceFile).Length;
                        var targetFileSize = new FileInfo(targetFile).Length;
                        if (sourceFileSize != targetFileSize)
                            copy = true;
                    }
                }
                else
                {
                    // target file does not exist
                    copy = true;
                }

                if (copy)
                {
                    File.Copy(sourceFile, targetFile, true);
                }
                else
                {
                    if (_deepLog)  Processing?.Invoke(this, new ExporterProcessStateEventArgs { Message = $"Skipping copying file '{sourceFile}' because it has the same last write time and size as the target file.", State = ExporterProcessStateEventArgs.ProcessStates.OnlyLog });
                }

            }

            await Task.CompletedTask;

            return new CloneResult();
        }
    }
}

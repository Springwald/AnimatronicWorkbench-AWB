// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License


namespace Awb.Core.Export
{
    public class Esp32ClientRemoteExporter : IExporter
    {
        private readonly string _esp32ClientsSourceFolder;

        public event EventHandler<ExporterProcessStateEventArgs>? Processing;

        public Esp32ClientRemoteExporter(string esp32ClientsSourceFolder)
        {
            _esp32ClientsSourceFolder = esp32ClientsSourceFolder;
        }

        public async Task<IExporter.ExportResult> Export(string targetPath)
        {
            var remoteSrcFolder = Path.Combine(_esp32ClientsSourceFolder, "awb_esp32_remote-controller");
            var targetSrcFolder = Path.Combine(targetPath, "awb_esp32_remote-controller");

            var cloneResult = await new SrcFolderCloner(remoteSrcFolder, targetSrcFolder, removeExtraFilesInTarget: false).Clone();
            if (!cloneResult.Success)
            {
                Processing?.Invoke(this, new ExporterProcessStateEventArgs { ErrorMessage = cloneResult.ErrorMessage });
                return new IExporter.ExportResult { ErrorMessage = cloneResult.ErrorMessage };
            }

            return IExporter.ExportResult.SuccessResult;
        }
    }
}

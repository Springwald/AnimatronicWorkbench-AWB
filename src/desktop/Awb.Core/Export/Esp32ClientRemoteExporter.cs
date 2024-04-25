// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License


using Awb.Core.Export.ExporterParts;

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

        public async Task<IExporter.ExportResult> ExportAsync(string targetPath)
        {
            var remoteSrcFolder = Path.Combine(_esp32ClientsSourceFolder, "awb_esp32_remote-controller");

            if (!Directory.Exists(remoteSrcFolder))
            {
                return new IExporter.ExportResult { ErrorMessage = $"Source folder '{remoteSrcFolder}' not found" };
            }

            var cloneResult = await new SrcFolderCloner(remoteSrcFolder, targetPath, removeExtraFilesInTarget: false).Clone();
            if (!cloneResult.Success)
            {
                Processing?.Invoke(this, new ExporterProcessStateEventArgs { ErrorMessage = cloneResult.ErrorMessage });
                return new IExporter.ExportResult { ErrorMessage = cloneResult.ErrorMessage };
            }

            var dataExporter = new[]
            {
                new WifiConfigExporter(new WifiConfigData(wlanSSID: "test", wlanPassword: "1234567")),
            };

            foreach(var exporter in dataExporter)
            {
                var result = await exporter.ExportAsync(targetPath);
                if (result != IExporter.ExportResult.SuccessResult)
                {
                    Processing?.Invoke(this, new ExporterProcessStateEventArgs { ErrorMessage = result.ErrorMessage });
                    return result;
                }
            }   

            return IExporter.ExportResult.SuccessResult;
        }

    }
}
